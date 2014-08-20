Hangfire.Highlighter
====================

Sample project for [Hangfire](http://hangfire.io)'s [Highlighter tutorial](http://docs.hangfire.io/en/latest/tutorials/highlight.html).

Consider you are building a code snippet gallery web application like GitHub Gists, and want to implement the syntax highlighting feature. To improve user experience, you are also want it to work even if a user disabled JavaScript in her browser.

To support this scenario and to reduce the project development time, you choosed to use a web service for syntax highlighting, such as http://pygments.appspot.com or http://www.hilite.me.

Learning Points
----------------

* Sometimes you can’t avoid long-running methods in ASP.NET applications.
* Long running methods can cause your application to be un-responsible from the users point of view.
* To remove waits you should place your long-running method invocation into background job.
* Background job processing is complex itself, but simple with Hangfire.
* You can process background jobs even inside ASP.NET applications with Hangfire.
