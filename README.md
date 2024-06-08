# QuickTicketStoreDemo

## 🚀 Session Management with Custom ITicketStore Implementation 🚀

As a Full Stack Developer with about a year of experience, I've recently revisited an ambitious project idea from my college days. The goal was to customize the identity package in .NET Core by storing comprehensive session information, such as IP address, browser name, and version, in a normal database or in-memory database such as Redis. This would allow users to manage and monitor their active sessions effectively.

Back then, my knowledge and my team's knowledge were limited, making this idea feel out of reach. However, with my current experience, I successfully implemented this functionality using the ITicketStore interface.

Here's a brief overview of the approach:

- **Custom ITicketStore Implementation**: By creating a contract class that implements ITicketStore, I could set it as the SessionStore. This class handles all necessary tasks, storing issued tickets in Redis or a SQL Server.

- **Redis In-Memory Database**: Initially, I used Redis to store session data. You can check out the source code on GitHub.

- **Live Demo**: Try out the [live demo](http://quickticketstoredemo.runasp.net/). Note that it's hosted on a free server without SSL, so please use HTTP instead of HTTPS. Due to limitations of the free hosting service, I've adapted the source code to store tickets in a SQL Server for this demo.

This project has been a rewarding experience, enhancing my understanding of session management and custom implementation in .NET Core. I'd love to hear your thoughts and any feedback you might have!
