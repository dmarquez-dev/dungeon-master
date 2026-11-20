import re,sys
# BP: bold-marker parity per AMENDED block. Opener: first line containing '[AMENDED' after the previous block closed.
# Closer: first later line whose rstrip ends with ']**' or ']'. Fenced code skipped; inline code spans removed;
# '*`' and '`*' (italic around code) neutralised so they do not leave a fake '**'.
def blocks(path):
    L=open(path,'rb').read().decode('utf-8').split('\r\n')
    out=[];start=None;fence=False;cnt=0
    for i,l in enumerate(L,1):
        if l.startswith('```'):
            fence=not fence; continue
        if fence: continue
        if start is None and '[AMENDED' in l and l.lstrip().startswith('**['):
            start=i;cnt=0
        if start is not None:
            s=re.sub(r'`[^`]*`','C',l)
            cnt+=s.count('**')
            r=l.rstrip()
            if r.endswith(']**') or (r.endswith(']') and i>start):
                out.append((start,i,cnt,r[-12:]));start=None
    return out
for p in sys.argv[1:]:
    b=blocks(p); odd=[x for x in b if x[2]%2]
    print(p.split('/')[-1],'blocks',len(b),'odd',len(odd))
    for x in odd: print('  ODD',x)
