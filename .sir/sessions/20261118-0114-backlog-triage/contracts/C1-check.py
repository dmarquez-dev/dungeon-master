# C1-check.py -- operative definition of contracts/C1-triage-record.md and the grammar of
# contracts/C2-gate2-decisions.md (session 20261118-0114-backlog-triage). Run with `python -I`.
#
#   python -I C1-check.py populations
#   python -I C1-check.py self-test
#   python -I C1-check.py record <record file>
#   python -I C1-check.py c2
#   python -I C1-check.py union <triage dir>
#   python -I C1-check.py apply <worktree root>
#
# Each mode prints one line per finding and ends with one summary line beginning `C1-check <mode>:`.
# Exit status is 0 when the summary reports 0 errors (and, for c2, 0 pending), 1 otherwise.
import hashlib
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
C2_PATH = os.path.join(HERE, 'C2-gate2-decisions.md')
MAIN = 'C:/Users/dmarq/Development/Repos/dungeon-master/.sir/'
ORIG_BACKLOG = MAIN + 'backlog.md'
ORIG_LESSONS = MAIN + 'lessons.md'
SHA_BACKLOG = '5cfbfb9d8e76ed0dc26182f7b9772adc4c27794dd6a3592092905e24f0a3fe84'
SHA_LESSONS = '6d185c70424d4b7dfbe60b6eff571447f80ac08bb81b849235e431664420adbe'
SESSION = '20261118-0114-backlog-triage'
FENCE = '~~~~'
RANGES = {
	'rows-0007-0030.md': (7, 30),
	'rows-0034-0058.md': (34, 58),
	'rows-0062-0087.md': (62, 87),
	'rows-0091-0109.md': (91, 109),
	'rows-0110-0137.md': (110, 137),
	'rows-0145-0163.md': (145, 163),
}
DISPOSITIONS = ('remove', 'keep', 'rewrite', 'lesson', 'decided')
ROW_RE = re.compile(r'^\d{4}-\d{2}-\d{2} \S+ ')
LINE_REF = re.compile(r'(?<!dir-sdlc/)backlog\.md:\d|\b(?:row|line)s? :?\d+\b(?!-)')
LESSON_RE = re.compile(
	r'^(\d{4}-\d{2}-\d{2}) (\S+) \[(project|framework|harness)\] (.+) '
	r'\(source: \.sir/backlog\.md row (\S+) (.+), routed by ' + re.escape(SESSION) + r'\)$')
TAIL_MARK = ' -- source: '
# rows discussion.md names as human or Appium checks: rewrite to numbered steps, Pass:, anchor at a commit
HUMAN_ROWS = (25, 52, 159)
ANCHOR_RE = re.compile(r'[\w./-]+:\d+(?:-\d+)?`? at `?[0-9a-f]{7,40}\b')


def human_form_errors(t):
	errs = []
	if t is None:
		return ['human-check row needs rewrite new-text']
	for need in ('(1) ', '(2) ', 'Pass:'):
		if need not in t:
			errs.append('human-check rewrite lacks "' + need + '"')
	if not ANCHOR_RE.search(t.split(TAIL_MARK)[0]):
		errs.append('human-check rewrite lacks a <path>:<line> anchor stated at <commit>')
	return errs
LINE3_ADD = ('Their re-measurement at `4d874be`, with each open entry\'s disposition and evidence, '
	'lives in `.sir/sessions/20261118-0114-backlog-triage/triage/`.')


def lines_of(b):
	t = b.decode('utf-8')
	parts = t.split('\r\n')
	if parts and parts[-1] == '':
		parts = parts[:-1]
	return parts


def text_lines(path):
	# record files and C2 may be LF or CRLF
	t = open(path, 'rb').read().decode('utf-8').replace('\r\n', '\n')
	parts = t.split('\n')
	if parts and parts[-1] == '':
		parts = parts[:-1]
	return parts


def load_originals():
	b = open(ORIG_BACKLOG, 'rb').read()
	l = open(ORIG_LESSONS, 'rb').read()
	if hashlib.sha256(b).hexdigest() != SHA_BACKLOG or hashlib.sha256(l).hexdigest() != SHA_LESSONS:
		print('ERROR reference files moved: main-checkout backlog.md or lessons.md no longer has its 4d874be sha256')
		sys.exit(1)
	return b, l


def open_rows(blines):
	rows = {}
	head = None
	for i, s in enumerate(blines, 1):
		if s.startswith('## '):
			head = s
		elif ROW_RE.match(s) and head != '## Permanently deferred':
			rows[i] = s
	return rows


def split_row(s):
	date, session, rest = s.split(' ', 2)
	k = rest.index(TAIL_MARK)
	return date, session, rest[:k], rest[k:]


def check_rewrite(orig, new):
	errs = []
	d, s, a, t = split_row(orig)
	if not new.startswith(d + ' ' + s + ' '):
		return ['rewrite does not begin with the row date and session']
	rest = new[len(d) + len(s) + 2:]
	if rest.count(TAIL_MARK) != 1:
		return ['rewrite must contain " -- source: " exactly once']
	k = rest.index(TAIL_MARK)
	na, nt = rest[:k], rest[k:]
	if not na.strip():
		errs.append('rewrite action is empty')
	if na == a:
		errs.append('rewrite action equals the original action')
	if nt != t and nt != t.replace('records undetermined; ', '', 1):
		errs.append('rewrite tail differs from the original tail')
	if LINE_REF.search(na):
		errs.append('rewrite action names a row by line: ' + LINE_REF.search(na).group(0))
	return errs


def check_lesson(orig, new):
	d, s, a, t = split_row(orig)
	m = LESSON_RE.match(new)
	if not m:
		return ['lesson line does not match the section 5 shape']
	errs = []
	if m.group(1) != d or m.group(2) != s or m.group(5) != s:
		errs.append('lesson line date/session differ from the row')
	if LINE_REF.search(new):
		errs.append('lesson line names a row by line: ' + LINE_REF.search(new).group(0))
	return errs


def read_fence(L, i, errs, where):
	# L[i] must be FENCE; returns (content lines, index after closing fence)
	if i >= len(L) or L[i] != FENCE:
		errs.append(where + ': expected opening ' + FENCE)
		return None, i
	j = i + 1
	out = []
	while j < len(L) and L[j] != FENCE:
		out.append(L[j])
		j += 1
	if j >= len(L):
		errs.append(where + ': unclosed fence')
		return None, j
	return out, j + 1


def parse_field(L, i, key, errs, where):
	pre = '- ' + key + ': '
	if i < len(L) and L[i].startswith(pre):
		return L[i][len(pre):], i + 1
	errs.append(where + ': expected field "' + pre.strip() + '" at line ' + str(i + 1))
	return None, i + 1


def parse_newtext(L, i, errs, where):
	if i < len(L) and L[i] == '- new-text: -':
		return None, i + 1
	if i < len(L) and L[i] == '- new-text:':
		c, j = read_fence(L, i + 1, errs, where + ' new-text')
		if c is not None and len(c) != 1:
			errs.append(where + ': new-text fence must hold exactly one line')
			return None, j
		return (c[0] if c else None), j
	errs.append(where + ': expected "- new-text: -" or "- new-text:"')
	return None, i + 1


def parse_blocks(L, start, errs):
	blocks = []
	i = start
	while i < len(L):
		if L[i] == '':
			i += 1
			continue
		m = re.match(r'^### row ([1-9]\d*)$', L[i])
		if not m:
			errs.append('line ' + str(i + 1) + ': expected "### row <N>" or a blank line')
			i += 1
			continue
		n = int(m.group(1))
		w = 'row ' + str(n)
		i += 1
		b = {'n': n}
		for key in ('date', 'session', 'disposition', 'decision', 'lessons-line', 'xref', 'summary'):
			b[key], i = parse_field(L, i, key, errs, w)
		if i < len(L) and L[i] == '- row:':
			b['row'], i = read_fence(L, i + 1, errs, w + ' row')
		else:
			errs.append(w + ': expected "- row:"')
			b['row'] = None
		if i < len(L) and L[i] == '- evidence:':
			b['evidence'], i = read_fence(L, i + 1, errs, w + ' evidence')
		else:
			errs.append(w + ': expected "- evidence:"')
			b['evidence'] = None
		b['new-text'], i = parse_newtext(L, i, errs, w)
		blocks.append(b)
	return blocks


def parse_c2(errs):
	L = text_lines(C2_PATH)
	try:
		i = L.index('## Entries') + 1
	except ValueError:
		errs.append('C2: no "## Entries" heading')
		return {}, {}
	entries = {}
	subs = {}
	cur = None
	while i < len(L):
		s = L[i]
		if s == '':
			i += 1
			continue
		m = re.match(r'^### (D[1-9]\d*)$', s)
		if m:
			cur = m.group(1)
			if cur in entries:
				errs.append('C2: duplicate entry ' + cur)
			e = {'id': cur}
			i += 1
			for key in ('rows', 'subject', 'premise', 'answer'):
				e[key], i = parse_field(L, i, key, errs, 'C2 ' + cur)
			try:
				e['rowset'] = [int(x) for x in (e['rows'] or '').split(', ')]
			except ValueError:
				errs.append('C2 ' + cur + ': rows is not a ", "-separated list of line numbers')
				e['rowset'] = []
			entries[cur] = e
			continue
		m = re.match(r'^#### row ([1-9]\d*)$', s)
		if m and cur:
			n = int(m.group(1))
			w = 'C2 ' + cur + ' row ' + str(n)
			i += 1
			sb = {'n': n, 'entry': cur}
			sb['disposition'], i = parse_field(L, i, 'disposition', errs, w)
			sb['lessons-line'], i = parse_field(L, i, 'lessons-line', errs, w)
			sb['new-text'], i = parse_newtext(L, i, errs, w)
			if n in subs:
				errs.append(w + ': row already named by ' + subs[n]['entry'])
			subs[n] = sb
			continue
		errs.append('C2 line ' + str(i + 1) + ': unexpected text')
		i += 1
	return entries, subs


def check_disposition(n, disp, lline, new, orig, llines, errs, w, allow_decided):
	allowed = DISPOSITIONS if allow_decided else DISPOSITIONS[:4]
	if disp not in allowed:
		errs.append(w + ': disposition "' + str(disp) + '" not one of ' + '|'.join(allowed))
		return
	if lline != '-' and disp != 'lesson':
		errs.append(w + ': lessons-line given without disposition lesson')
	if disp == 'rewrite':
		if new is None:
			errs.append(w + ': rewrite needs new-text')
		else:
			errs.extend(w + ': ' + e for e in check_rewrite(orig, new))
	elif disp == 'lesson':
		if lline == '-':
			if new is None:
				errs.append(w + ': lesson needs new-text or lessons-line')
			else:
				errs.extend(w + ': ' + e for e in check_lesson(orig, new))
		else:
			if new is not None:
				errs.append(w + ': lesson with lessons-line must have new-text -')
			if not re.match(r'^[1-9]\d*$', str(lline)) or not (16 <= int(lline) <= len(llines)) \
					or not ROW_RE.match(llines[int(lline) - 1]):
				errs.append(w + ': lessons-line ' + str(lline) + ' is not a lesson entry line of ORIGINAL LESSONS')
	elif new is not None:
		errs.append(w + ': new-text must be - for ' + disp)


def check_record_lines(L, name, rows, llines, entries, errs):
	if name not in RANGES:
		errs.append('file name ' + name + ' is not one of the six section 2 files')
		return []
	a, z = RANGES[name]
	want = '# Triage: .sir/backlog.md lines ' + str(a) + '-' + str(z) + ' at 4d874be'
	if not L or L[0] != want:
		errs.append('line 1 is not exactly: ' + want)
	start = 1
	while start < len(L) and not L[start].startswith('### '):
		start += 1
	blocks = parse_blocks(L, start, errs)
	pop = [n for n in sorted(rows) if a <= n <= z]
	got = [b['n'] for b in blocks]
	if got != pop:
		errs.append('block keys ' + str(got) + ' differ from the population ' + str(pop))
	for b in blocks:
		n = b['n']
		w = 'row ' + str(n)
		if n not in rows:
			continue
		orig = rows[n]
		d, s, act, tail = split_row(orig)
		if b['date'] != d or b['session'] != s:
			errs.append(w + ': date/session differ from the row')
		if b['row'] != [orig]:
			errs.append(w + ': row fence is not the row line verbatim')
		ev = b['evidence'] or []
		if not any(x.startswith('$ ') for x in ev):
			errs.append(w + ': evidence has no line beginning "$ "')
		for k, x in enumerate(ev):
			if re.match(r'^\$ (?:git|dotnet)(?: |$)', x) and not (k + 1 < len(ev) and ev[k + 1].startswith('NOT RUN:')):
				errs.append(w + ': evidence runs git or dotnet (C1 section 7): ' + x)
		if not (b['summary'] or '').strip():
			errs.append(w + ': summary is empty')
		x = b['xref']
		if x != '-':
			try:
				xs = [int(v) for v in (x or '').split(', ')]
				if xs != sorted(set(xs)) or n in xs or any(v not in rows for v in xs):
					raise ValueError
			except ValueError:
				errs.append(w + ': xref must be - or ascending open-row lines other than N')
		disp = b['disposition']
		dec = b['decision']
		if disp == 'decided':
			e = entries.get(dec)
			if e is None or n not in e['rowset']:
				errs.append(w + ': decision ' + str(dec) + ' is not a C2 entry listing this row')
			if b['lessons-line'] != '-' or b['new-text'] is not None:
				errs.append(w + ': decided needs lessons-line - and new-text -')
		else:
			if dec != '-':
				errs.append(w + ': decision must be - unless disposition is decided')
			if any(n in e['rowset'] for e in entries.values()):
				errs.append(w + ': row is listed by C2 and must be decided')
			check_disposition(n, disp, b['lessons-line'], b['new-text'], orig, llines, errs, w, True)
		if n in HUMAN_ROWS:
			if disp != 'rewrite':
				errs.append(w + ': human-check row must be rewrite (discussion.md Decisions)')
			errs.extend(w + ': ' + e for e in human_form_errors(b['new-text']))
	return blocks


def mode_record(path):
	b, l = load_originals()
	rows = open_rows(lines_of(b))
	errs = []
	entries, subs = parse_c2([])
	blocks = check_record_lines(text_lines(path), os.path.basename(path), rows, lines_of(l), entries, errs)
	for e in errs:
		print('ERROR ' + e)
	tally = {}
	for x in blocks:
		tally[x['disposition']] = tally.get(x['disposition'], 0) + 1
	print('C1-check record: ' + str(len(blocks)) + ' blocks, ' + str(len(errs)) + ' errors, dispositions '
		+ ', '.join(k + '=' + str(tally[k]) for k in sorted(tally)))
	return 0 if not errs else 1


def mode_c2():
	b, l = load_originals()
	rows = open_rows(lines_of(b))
	llines = lines_of(l)
	errs = []
	entries, subs = parse_c2(errs)
	decide = sorted(n for n, s in rows.items() if re.match(r'^\S+ \S+ Decide ', s))
	listed = sorted(n for e in entries.values() for n in e['rowset'])
	if listed != decide:
		errs.append('C2 rows ' + str(listed) + ' differ from the Decide rows ' + str(decide))
	pending = 0
	for e in entries.values():
		for n in e['rowset']:
			if n not in subs or subs[n]['entry'] != e['id']:
				errs.append('C2 ' + e['id'] + ': no "#### row ' + str(n) + '" sub-block')
	for n, sb in sorted(subs.items()):
		w = 'C2 ' + sb['entry'] + ' row ' + str(n)
		if n not in rows:
			errs.append(w + ': not an open row')
			continue
		if sb['disposition'] == '(pending)':
			pending += 1
			continue
		check_disposition(n, sb['disposition'], sb['lessons-line'], sb['new-text'], rows[n], llines, errs, w, False)
	for e in entries.values():
		for key in ('premise', 'answer'):
			if e[key] == '(pending)':
				pending += 1
	for e in errs:
		print('ERROR ' + e)
	print('C1-check c2: ' + str(len(entries)) + ' entries, ' + str(len(listed)) + ' decide rows, '
		+ str(len(subs)) + ' row sub-blocks, ' + str(pending) + ' pending, ' + str(len(errs)) + ' errors')
	return 0 if not errs and not pending else 1


def all_blocks(tdir, errs):
	b, l = load_originals()
	rows = open_rows(lines_of(b))
	entries, subs = parse_c2([])
	out = {}
	dup = 0
	for name in sorted(RANGES):
		p = os.path.join(tdir, name)
		if not os.path.exists(p):
			errs.append('missing record file ' + name)
			continue
		ferrs = []
		for x in check_record_lines(text_lines(p), name, rows, lines_of(l), entries, ferrs):
			if x['n'] in out:
				dup += 1
			out[x['n']] = x
		errs.extend(name + ': ' + e for e in ferrs)
	return rows, out, dup


def mode_union(tdir):
	errs = []
	rows, blocks, dup = all_blocks(tdir, errs)
	missing = sorted(set(rows) - set(blocks))
	extra = sorted(set(blocks) - set(rows))
	for e in errs:
		print('ERROR ' + e)
	if missing:
		print('MISSING ' + str(missing))
	if extra:
		print('EXTRA ' + str(extra))
	print('C1-check union: ' + str(len(blocks)) + ' keys, ' + str(len(missing)) + ' missing, ' + str(len(extra))
		+ ' extra, ' + str(dup) + ' duplicated, ' + str(len(errs)) + ' errors')
	return 0 if not (errs or missing or extra or dup) else 1


def expected(rows, blocks, subs, blines, llb):
	final = {}
	for n in rows:
		if n in subs:
			sb = subs[n]
			final[n] = (sb['disposition'], sb['new-text'])
		else:
			x = blocks[n]
			final[n] = (x['disposition'], x['new-text'])
	out = []
	for i, s in enumerate(blines, 1):
		if i == 3:
			out.append(s + ' ' + LINE3_ADD)
			continue
		if i in final:
			d, t = final[i]
			if d in ('remove', 'lesson'):
				continue
			out.append(t if d == 'rewrite' else s)
			continue
		out.append(s)
	# drop emptied headings with the blank line after each
	res = []
	k = 0
	while k < len(out):
		s = out[k]
		if s.startswith('## '):
			j = k + 1
			has = False
			while j < len(out) and not out[j].startswith('## '):
				if ROW_RE.match(out[j]):
					has = True
					break
				j += 1
			if not has:
				k += 2 if k + 1 < len(out) and out[k + 1] == '' else 1
				continue
		res.append(s)
		k += 1
	col = []
	for s in res:
		if s == '' and col and col[-1] == '':
			continue
		col.append(s)
	while col and col[-1] == '':
		col.pop()
	eb = ''.join(s + '\r\n' for s in col).encode('utf-8')
	app = [final[n][1] for n in sorted(final) if final[n][0] == 'lesson' and final[n][1] is not None]
	el = llb + ''.join(s + '\r\n' for s in app).encode('utf-8')
	return eb, el, final


def diffcount(a, b):
	la, lb = a.split(b'\r\n'), b.split(b'\r\n')
	n = sum(1 for x, y in zip(la, lb) if x != y) + abs(len(la) - len(lb))
	return n


def counts(b):
	return str(b.count(b'\r\n')) + '/' + str(b.count(b'\n')) + '/' + str(b.count(b'\r'))


def mode_apply(wt):
	errs = []
	tdir = os.path.join(wt, '.sir', 'sessions', SESSION, 'triage')
	rows, blocks, dup = all_blocks(tdir, errs)
	c2errs = []
	entries, subs = parse_c2(c2errs)
	errs.extend(c2errs)
	if any(sb['disposition'] == '(pending)' for sb in subs.values()):
		errs.append('C2 has pending sub-blocks')
	for n in rows:
		if n not in blocks:
			errs.append('row ' + str(n) + ' has no record block')
		elif blocks[n]['disposition'] == 'decided' and n not in subs:
			errs.append('row ' + str(n) + ' is decided but C2 has no sub-block for it')
	if errs:
		for e in errs:
			print('ERROR ' + e)
		print('C1-check apply: not computed, ' + str(len(errs)) + ' errors')
		return 1
	b, l = load_originals()
	eb, el, final = expected(rows, blocks, subs, lines_of(b), l)
	wb = open(os.path.join(wt, '.sir', 'backlog.md'), 'rb').read()
	wl = open(os.path.join(wt, '.sir', 'lessons.md'), 'rb').read()
	db, dl = diffcount(eb, wb), diffcount(el, wl)
	tally = {}
	for d, t in final.values():
		tally[d] = tally.get(d, 0) + 1
	appended = sum(1 for d, t in final.values() if d == 'lesson' and t is not None)
	# second predicate, by membership rather than by construction
	wlines = lines_of(wb)
	mem = 0
	for n, (d, t) in sorted(final.items()):
		o = rows[n]
		if d in ('remove', 'lesson') and wlines.count(o) != 0:
			mem += 1
			print('MEMBERSHIP row ' + str(n) + ' (' + d + ') still present')
		elif d == 'keep' and wlines.count(o) != 1:
			mem += 1
			print('MEMBERSHIP row ' + str(n) + ' (keep) present ' + str(wlines.count(o)) + ' times')
		elif d == 'rewrite' and (wlines.count(o) != 0 or wlines.count(t) != 1):
			mem += 1
			print('MEMBERSHIP row ' + str(n) + ' (rewrite) original/new present ' + str(wlines.count(o)) + '/' + str(wlines.count(t)))
	# layout properties of C1 section 6 step 3
	lay = 0
	for k, s in enumerate(wlines):
		if s == '' and k > 0 and wlines[k - 1] == '':
			lay += 1
			print('LAYOUT consecutive blank lines at line ' + str(k + 1))
		if s.startswith('## ') and not (k + 2 < len(wlines) and wlines[k + 1] == '' and ROW_RE.match(wlines[k + 2])):
			lay += 1
			print('LAYOUT heading not followed by one blank line and a row: ' + s)
	if not wlines or wlines[-1] == '':
		lay += 1
		print('LAYOUT file ends with a blank line')
	print('final dispositions ' + ', '.join(k + '=' + str(tally[k]) for k in sorted(tally))
		+ '; lessons appended ' + str(appended) + '; membership ' + str(mem) + ' violations; layout '
		+ str(lay) + ' violations')
	print('C1-check apply: backlog ' + str(db) + ' differing lines, lessons ' + str(dl)
		+ ' differing lines, CRLF/LF/CR backlog ' + counts(wb) + ' lessons ' + counts(wl))
	return 0 if db == 0 and dl == 0 and mem == 0 and lay == 0 else 1


def mode_populations():
	b, l = load_originals()
	rows = open_rows(lines_of(b))
	tot = 0
	for name in sorted(RANGES):
		a, z = RANGES[name]
		pop = [n for n in rows if a <= n <= z]
		tot += len(pop)
		print(name + ' lines ' + str(a) + '-' + str(z) + ': ' + str(len(pop)) + ' open rows')
	print('C1-check populations: ' + str(len(rows)) + ' open rows, ' + str(tot) + ' in the six ranges')
	return 0 if tot == len(rows) == 132 else 1


def block_text(n, orig, disp, new, ev):
	d, s, a, t = split_row(orig)
	out = ['### row ' + str(n), '- date: ' + d, '- session: ' + s, '- disposition: ' + disp,
		'- decision: -', '- lessons-line: -', '- xref: -', '- summary: fixture.', '- row:', FENCE, orig, FENCE,
		'- evidence:', FENCE] + ev + [FENCE]
	out += ['- new-text: -'] if new is None else ['- new-text:', FENCE, new, FENCE]
	return out


def mode_self_test():
	b, l = load_originals()
	rows = open_rows(lines_of(b))
	llines = lines_of(l)
	results = []

	def case(label, want_ok, L, name):
		errs = []
		sub = {r: rows[r] for r in rows}
		blocks = parse_blocks(L, 0, errs)
		for x in blocks:
			n = x['n']
			if x['row'] != [sub[n]]:
				errs.append('row fence mismatch')
			check_disposition(n, x['disposition'], x['lessons-line'], x['new-text'], sub[n], llines, errs, 'row', True)
		ok = not errs
		results.append((label, ok == want_ok, errs))

	bs = chr(92)
	r87, r128 = rows[87], rows[128]
	assert '|' in r87 and bs in r128
	ev = ['$ grep -n MASTER-051 docs/audit/00-master-index.md', 'a|b ' + bs + '| ' + bs + bs]
	case('pipe row 87 round-trips as keep', True, block_text(87, r87, 'keep', None, ev), '')
	case('backslash row 128 round-trips as keep', True, block_text(128, r128, 'keep', None, ev), '')
	d, s, a, t = split_row(r87)
	good_rw = d + ' ' + s + ' Escape the two raw pipes in `MASTER-051`' + "'" + 's status cell as `' + bs + '|`, re-measured. ' + t
	case('rewrite carrying ' + bs + '| is accepted', True, block_text(87, r87, 'rewrite', good_rw, ev), '')
	L = block_text(87, r87, 'rewrite', good_rw, ev)
	errs = []
	got = parse_blocks(L, 0, errs)[0]['new-text']
	results.append(('rewrite new-text parses back byte-identical', got == good_rw and not errs, errs))
	case('rewrite naming row :147 is rejected', False,
		block_text(87, r87, 'rewrite', d + ' ' + s + ' See row :147 first. ' + t, ev), '')
	case('rewrite naming .sir/backlog.md:12 is rejected', False,
		block_text(87, r87, 'rewrite', d + ' ' + s + ' See .sir/backlog.md:12 first. ' + t, ev), '')
	case('rewrite naming row 2026-09-14-make-the-floor-sidebar-operable is accepted', True,
		block_text(87, r87, 'rewrite', d + ' ' + s + ' See the row 2026-09-14-make-the-floor-sidebar-operable first. ' + t, ev), '')
	case('rewrite with a changed tail is rejected', False,
		block_text(87, r87, 'rewrite', d + ' ' + s + ' New words. -- source: elsewhere.', ev), '')
	d2, s2, a2, t2 = split_row(r128)
	good_ls = (d2 + ' ' + s2 + ' [harness] Count lines with `b.count(b' + "'" + bs + 'n' + "'" + ')`. '
		+ '(source: .sir/backlog.md row ' + s2 + ' line-count method, routed by ' + SESSION + ')')
	case('lesson line in section 5 shape is accepted', True, block_text(128, r128, 'lesson', good_ls, ev), '')
	case('lesson line with the wrong session is rejected', False,
		block_text(128, r128, 'lesson', good_ls.replace(' ' + s2 + ' [', ' 2026-01-01-x [', 1), ev), '')
	case('lesson line without a tag is rejected', False,
		block_text(128, r128, 'lesson', good_ls.replace('[harness] ', '', 1), ev), '')
	L = block_text(87, r87, 'keep', None, ev)
	L.remove(FENCE)
	case('a missing fence line is rejected', False, L, '')
	L = block_text(87, r87, 'keep', None, ev)
	L[1], L[2] = L[2], L[1]
	case('fields out of order are rejected', False, L, '')
	case('keep with new-text is rejected', False, block_text(87, r87, 'keep', good_rw, ev), '')
	r25 = rows[25]
	d3, s3, a3, t3 = split_row(r25)
	hum = (d3 + ' ' + s3 + ' Measure the floor-name column: (1) launch the app; (2) read the width of the column at '
		+ '`src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml:12` at `4d874be`. Pass: it is wider. ' + t3)
	results.append(('human-check form accepted', human_form_errors(hum) == [], human_form_errors(hum)))
	results.append(('human-check form without Pass: rejected', human_form_errors(hum.replace('Pass:', 'Then')) != [], []))
	results.append(('human-check form without a commit anchor rejected',
		human_form_errors(hum.replace(' at `4d874be`', '')) != [], []))
	results.append(('LINE-REF matches "row :147"', bool(LINE_REF.search('row :147')), []))
	results.append(('LINE-REF matches ".sir/backlog.md:12"', bool(LINE_REF.search('.sir/backlog.md:12')), []))
	results.append(('LINE-REF does not match ".dir-sdlc/backlog.md:351"', not LINE_REF.search('.dir-sdlc/backlog.md:351'), []))
	fail = [r for r in results if not r[1]]
	for label, passed, errs in results:
		print(('PASS ' if passed else 'FAIL ') + label + ('' if passed else ' ' + str(errs)))
	print('C1-check self-test: ' + str(len(results) - len(fail)) + ' passed, ' + str(len(fail)) + ' failed')
	return 0 if not fail else 1


def main(argv):
	if len(argv) < 2:
		print(__doc__ or 'usage: see header')
		return 2
	m = argv[1]
	if m == 'populations':
		return mode_populations()
	if m == 'self-test':
		return mode_self_test()
	if m == 'record' and len(argv) == 3:
		return mode_record(argv[2])
	if m == 'c2':
		return mode_c2()
	if m == 'union' and len(argv) == 3:
		return mode_union(argv[2])
	if m == 'apply' and len(argv) == 3:
		return mode_apply(argv[2])
	print('unknown mode')
	return 2


if __name__ == '__main__':
	sys.exit(main(sys.argv))
