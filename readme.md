Audio Device Quick-Switcher (ADQS)
==================================

Attention: Code Reviewers!
--------------------------

I'd appreciate C#-specific architectural/pattern/best-practice recommendations over fine-grain code review items. For example, 

* Should I be using XAML Data Binding everywhere instead of events?
* Does my UserControl (AudioDeviceListItem.xaml, AudioDeviceCheckBox.xaml) conform to best practices?
* Is my XAML terrible? I mean less terrible than XAML is normally?
* Is how I schedule work on the UI thread totally whack, bro? (UI.cs)
* I'm using Threads in places (KeyMonitor.cs, AudioSwitchQ.cs) and I think they might also be totally wrong.
* Is there a more C-sharpy way of implementing the KeyMonitor singleton?
* Am I using IDisposable correctly? (KeyMonitor.cs, AudioSwitchQ.cs) Mentally I have assigned `Foo.Dispose() == ~Foo()` but I think this might be a false assumption. A birdie told me Dispose might be called twice sometimes?!

This is a work in progress, currently in the "hacked together over a weekend" stage. Expect this to be a mess. After I get feedback on how to write better C#, I'll be de-tangling, refactoring and adding documentation.

Thanks!

Acknowledgements
----------------

This is a front-end for Belphemur's fantastic [AudioEndPointLibrary](https://github.com/Belphemur/AudioEndPointLibrary/).

Usage
-----

* Run ADQS
* Use Win+Alt+Space to switch Audio Device

Issues
------

None reported.

Backlog
-------

* Installer
* ReadMe
* Hot-key rebinding
* Code cleanup
* Help
* Tests
