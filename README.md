# Price-Digest-API-Integration

The purpose of this project was to integrate Price Digest into our company's website. This was done by writing code in C# in order to do the API calls. 
In this project, I established a database connection, creating error logging, and would pull the data from the API calls and store them into our SQL Server tables. 
From there, I'd take the data and create numerous stored procedures that were being used in order to insert, update, and select the data for display. 
I hope you enjoy this project I was able to put together for our company!

Please be aware that this was about 70% of the project. The other part of it I'm unable to share, as it's for our private website and is for internal use only. The other half did include some JavaScript and HTML/CSS changes, but
this at least was the most difficult/time-consuming part of the project.

This is a professional API I integrated for Pawnee Leasing Corporation. This was the first time I really had the opportunity to showcase my C# skills and professionally integrate an API with our website. 
If you're here for learning purposes, I hope this can be useful to you. If you're here just to check out what I've done, I really hope you enjoy it!





Key side notes for my project:

* You'll notice in my SQL queries that I use @ApplicationNum and @SubscriberID in all of them. This is because I work for a leasing company where we have
millions of applications. Those two variables are key information to trace each asset back to a specific application for an applicant.

* My GetRequests are what I used to plug in values that are within our tables so that I can send API requests based on that information.

* My ModelBuilders are what I'm using in order to run a query to get the data that's returned and plug it into my API requests.

* I did set up CallLogging in all of my classes, and you can see how i have that built by referencing the RequestFormats folder. This is also
  where you will find my initial Database Connection

* Lastly, my controllers are where you will actually see the API Calls happening.
