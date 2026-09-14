using Xunit;

// ONE APPIUM SESSION AT A TIME, AND IT IS NOT A PERFORMANCE PREFERENCE.
//
// Until this session the project held one test class, so xunit's default parallelism across
// collections never had a second collection to run. It does now. These tests drive the real
// desktop: each opens a DungeonMaster.App.exe window, and the tab-order walk sends Tab to
// whichever window holds the foreground keyboard focus. Two collections running at once would
// therefore have one test's key presses land in another test's window, which is not a state a
// test can create for itself or clean up after.
//
// The isolation rule this project keeps — every test opens and disposes its OWN session — buys
// order-independence, not immunity to a shared desktop. This attribute is the other half.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
