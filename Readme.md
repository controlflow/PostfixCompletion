﻿ReSharper Postfix Completion plugin
-----------------------------------

The idea is to prevent caret jumps backwards while typing C# code.

[See it in action](http://screencast.com/t/zqMDGTMDqhp)

#### Download

Pre-release package version is available in in ReSharper 8.0 Extension Manager gallery.
ReSharper 7.1 version will came shortly.

#### Available templates

Iterting over all kinds of collections, reverse iteration:

![foreach](/Content/foreach.png)

* `.arg`
* **`.await`**
* **.cast**

    expr.cast
    =>
    (SomeType) expr

* .null

    expr.null
    =>
    if (expr == null)

* .notnull
* **expr**`.foreach`

    foreach (var x in expr) {
    }

* expr`.for`

    for (var i = 0; i < expr.Length; i++) {
    }

* .forr
* .if
* .ifnot
* .not
* .field
* .prop
* .var
* .not
* .new
* .paren
* .return
* .throw
* .using
* .while

TODO:

* .lock
* work on statements?
** .try
** .if


// TODO: other templates

#### Feedback

Feel free to post any issues or feature requests in [JetBrains YouTrack](http://youtrack.jetbrains.com/issues/RSPL) (select *"PostfixCompletion"* subsystem).

Or contact me directly by email: *alexander.shvedov[at]jetbrains.com*