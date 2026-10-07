# MetroSharp

[توضیحات فارسی، کلیک کنید](README.fa.md)


> AI was used to polish this README file
    - author

> Project is not finished yet, however its not far from it.

### Backend, API, Automation & Telegram Infrastructure for Metro Moviez

MetroSharp is the server-side application behind **Metro Moviez**, a movie and TV-series distribution platform.

It provides a centralized environment for managing a large media library, discovering and downloading content, processing downloaded files, maintaining metadata, and exposing the library through APIs and client applications.

The system is built primarily with **ASP.NET Core 9**, following **Clean Architecture** and **CQRS** principles.

MetroSharp currently manages a library of **20,000+ movies**, with the architecture designed to support substantially larger collections.

---

## Table of Contents

* [Overview](#overview)
* [What MetroSharp Does](#what-metrosharp-does)
* [High-Level Architecture](#high-level-architecture)
* [How the System Works](#how-the-system-works)
* [Content Lifecycle](#content-lifecycle)
* [Manager / Administration Side](#manager--administration-side)
* [Client Side](#client-side)
* [Telegram Bot](#telegram-bot)
* [Mobile Application](#mobile-application)
* [REST API](#rest-api)
* [Torrent and Download System](#torrent-and-download-system)
* [Media Metadata](#media-metadata)
* [Database](#database)
* [Background Services](#background-services)
* [Project Structure](#project-structure)
* [Architecture](#architecture)
* [CQRS](#cqrs)
* [Clean Architecture](#clean-architecture)
* [Infrastructure Layer](#infrastructure-layer)
* [Persistence Layer](#persistence-layer)
* [Domain Layer](#domain-layer)
* [Application Layer](#application-layer)
* [API Layer](#api-layer)
* [Resource Layer](#resource-layer)
* [Authentication and Authorization](#authentication-and-authorization)
* [Logging and Monitoring](#logging-and-monitoring)
* [Reverse Proxy and Deployment](#reverse-proxy-and-deployment)
* [External Services](#external-services)
* [Typical Request Flow](#typical-request-flow)
* [Typical Download Flow](#typical-download-flow)
* [Design Goals](#design-goals)
* [Why the Architecture Is Structured This Way](#why-the-architecture-is-structured-this-way)
* [Future Development](#future-development)

---

# Overview

MetroSharp is the backend and automation platform used to operate Metro Moviez.

The system is responsible for more than simply providing a REST API.

It acts as an orchestration layer between several independent systems:

* ASP.NET Core Web API
* Database
* Torrent indexers
* Prowlarr
* qBittorrent
* Media processing services
* Telegram Bot
* Mobile application
* Web clients
* File storage
* Background workers

The goal is to keep these systems independent while providing a single application layer that coordinates them.

At a high level:

```text
                    ┌─────────────────────┐
                    │    MetroSharp API   │
                    └──────────┬──────────┘
                               │
              ┌────────────────┼────────────────┐
              │                │                │
              ▼                ▼                ▼
        Mobile App       Telegram Bot       Web Client
              │                │                │
              └────────────────┼────────────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │     MetroSharp      │
                    │   Business Logic    │
                    └──────────┬──────────┘
                               │
          ┌────────────────────┼────────────────────┐
          │                    │                    │
          ▼                    ▼                    ▼
      Database             qBittorrent          Prowlarr
          │                    │                    │
          │                    ▼                    │
          │              Downloaded Media           │
          │                    │                    │
          └────────────────────┼────────────────────┘
                               ▼
                         Media Library
```

---

# What MetroSharp Does

MetroSharp has several major responsibilities.

### 1. Media management

The application stores and manages information about:

* Movies
* TV series
* Seasons
* Episodes
* Genres
* Actors
* Directors
* Ratings
* Release information
* Posters and images
* Download information
* File information
* Availability

---

### 2. Content discovery

MetroSharp can communicate with external torrent/indexer infrastructure to find available media.

Prowlarr acts as the indexer management layer.

MetroSharp does not need to know the implementation details of every individual indexer.

Instead, it communicates with the external service responsible for searching them.

---

### 3. Download management

Once appropriate content is found, MetroSharp can interact with qBittorrent.

qBittorrent handles the actual torrent downloading.

MetroSharp acts as the orchestration layer around it.

This separation is intentional.

MetroSharp does not implement a torrent client.

Instead:

```text
MetroSharp
    │
    │ request download
    ▼
qBittorrent
    │
    │ download
    ▼
Cloud Storage
    │
    │ Store cloud Metadata
    ▼
Database
```

---

### 4. Media processing

After content has been downloaded, background processes can detect and process the resulting files.

Processing can include:

* Detecting downloaded files
* Matching files to media records
* Updating database state
* Extracting file information
* Moving or organizing files
* Preparing files for users
* Generating derived resources where necessary

---

### 5. API access

MetroSharp exposes a REST API consumed by client applications.

The API provides access to:

* Movies
* Series
* Search
* Categories
* Metadata
* Downloads
* User functionality
* Manager functionality
* System information

The API acts as the public boundary of the application.

---

### 6. Telegram integration

MetroSharp includes a Telegram bot that provides another client interface to the system.

Users can interact with the movie library through Telegram without directly communicating with the database or internal services.

---

### 7. Mobile management

A separate mobile application is planned/available for managing the server.

The mobile application communicates with MetroSharp through the REST API.

> Mobile application source code will be linked here when the repository is published.

---

# High-Level Architecture

MetroSharp follows a layered **Clean Architecture** design combined with **CQRS**.

```text
┌────────────────────────────────────────────────────┐
│                      Clients                       │
│                                                    │
│  Mobile App │ Telegram Bot │ Web Client │ Other    │
└───────────────────────┬────────────────────────────┘
                        │
                        ▼
┌────────────────────────────────────────────────────┐
│                        API                         │
│             HTTP / Controllers / Endpoints         │
└───────────────────────┬────────────────────────────┘
                        │
                        ▼
┌────────────────────────────────────────────────────┐
│                    Application                     │
│                                                    │
│ Commands │ Queries │ Handlers │ DTOs │ Behaviors   │
└───────────────┬───────────────────────┬────────────┘
                │                       │
                ▼                       ▼
┌────────────────────────┐    ┌──────────────────────┐
│         Domain         │    │    Infrastructure    │
│                        │    │                      │
│ Entities               │    │ Telegram             │
│ Value Objects/Enums    │    │ qBittorrent          │
│ Domain Rules           │    │ Prowlarr             │
│ Interfaces             │    │ Media Services       │
└────────────┬───────────┘    └──────────────────────┘
             │
             ▼
┌────────────────────────────────────────────────────┐
│                    Persistence                     │
│                                                    │
│ EF Core │ DbContexts │ Configurations │ Migrations │
└────────────────────────────────────────────────────┘
```

The important principle is that the core application does not depend directly on external technologies.

For example, business logic should not need to know whether torrent downloads are performed by qBittorrent, another torrent client, or a future implementation.

---

# How the System Works

MetroSharp can be understood as a central coordinator.

Instead of allowing every client to communicate directly with every service, MetroSharp sits between them.

Without MetroSharp:

```text
Mobile ──────► Database
Mobile ──────► qBittorrent
Mobile ──────► Prowlarr
Mobile ──────► Storage
Telegram ────► Database
Telegram ────► qBittorrent
```

This would make the system difficult to maintain and expose internal services unnecessarily.

Instead:

```text
                    MetroSharp
                 ┌──────────────┐
                 │              │
Mobile ─────────►│              │
Telegram ───────►│     API      │
Web ────────────►│              │
                 │              │
                 └──────┬───────┘
                        │
          ┌─────────────┼─────────────┐
          ▼             ▼             ▼
       Database     qBittorrent    Prowlarr
```

MetroSharp owns the business rules and coordinates the underlying systems.

---

# Content Lifecycle

One of the most important concepts in MetroSharp is the lifecycle of media.

A simplified movie lifecycle looks like this:

```text
Movie discovered
       │
       ▼
Metadata created
       │
       ▼
Search for available release
       │
       ▼
Torrent discovered
       │
       ▼
Download requested
       │
       ▼
qBittorrent downloads content
       │
       ▼
Download completed
       │
       ▼
MetroSharp detects/processes file
       │
       ▼
Media record updated
       │
       ▼
Movie becomes available
       │
       ├──────────────► REST API
       │
       ├──────────────► Mobile App
       │
       └──────────────► Telegram Bot
```

This separates **metadata availability** from **file availability**.

A movie can exist in the database even when its actual media file has not yet been downloaded.

---

# Manager / Administration Side

MetroSharp provides management functionality for administrators/managers.

A manager can interact with the system through management endpoints or the management application.

Typical manager operations include:

* Managing movies
* Managing series
* Managing users
* Monitoring downloads
* Starting downloads
* Inspecting download status
* Managing media records
* Managing metadata
* Managing system resources
* Managing Telegram-related functionality
* Inspecting background operations

The manager interface does not directly manipulate the database.

Instead:

```text
Manager
   │
   ▼
Management Client
   │
   ▼
REST API
   │
   ▼
Application Layer
   │
   ▼
Persistence / Infrastructure
```

This keeps business rules centralized.

---

# Client Side

Metro Moviez can expose the same backend through multiple clients.

Currently the main clients are:

### Mobile Application

A mobile client for interacting with the Metro Moviez server.

### Telegram Bot

A Telegram-based client for discovering and accessing content.

### Web Application

The API can also be consumed by a web-based frontend.

The important design decision is that clients are not responsible for implementing the application's business rules.

They communicate with MetroSharp through its API.

---

# Telegram Bot

The Telegram bot is an external client of MetroSharp.

Rather than implementing the complete movie-management logic inside the bot, the bot communicates with the application layer/API and acts primarily as a user interface.

Conceptually:

```text
Telegram User
      │
      ▼
Telegram
      │
      ▼
Metro Moviez Bot
      │
      ▼
MetroSharp
      │
      ├── Database
      ├── Media Library
      └── Download System
```

This allows Telegram functionality to evolve independently from the core application.

The bot can provide functionality such as:

* Movie search
* Movie discovery
* Movie information
* Download/access links
* User interaction
* Navigation through the movie library

The Telegram integration is implemented as an infrastructure concern rather than being embedded into the domain layer.

---

# Mobile Application

The mobile application is a separate client.

It does not contain the server's business logic.

Instead, it communicates with MetroSharp's REST API.

```text
┌─────────────────┐
│   Mobile App    │
└────────┬────────┘
         │ HTTPS
         ▼
┌─────────────────┐
│   MetroSharp    │
│      API        │
└────────┬────────┘
         │
         ▼
    Application
         │
         ▼
       Domain
```

This allows the mobile application to be replaced or extended without changing the underlying business logic.

---

# REST API

The API layer is the external HTTP boundary of MetroSharp.

Its responsibilities include:

* HTTP endpoints
* Request handling
* Authentication
* Authorization
* Request validation
* Response formatting
* Mapping API requests to application commands/queries

The API should remain relatively thin.

For example, a controller should not contain complex business logic.

Instead:

```text
HTTP Request
     │
     ▼
Controller
     │
     ▼
Command / Query
     │
     ▼
Handler
     │
     ▼
Domain / Infrastructure
     │
     ▼
Result
     │
     ▼
HTTP Response
```

This makes API endpoints easier to test and maintain.

---

# Torrent and Download System

MetroSharp does not act as the torrent engine.

Instead, torrent functionality is delegated to **qBittorrent**.

This separation provides several advantages:

* Torrent implementation is isolated
* qBittorrent can manage active downloads
* MetroSharp remains focused on business logic
* Download state can be monitored independently
* The torrent client can potentially be replaced later

The basic flow is:

```text
MetroSharp
    │
    │ Search
    ▼
Prowlarr
    │
    │ Results
    ▼
MetroSharp
    │
    │ Select release
    ▼
qBittorrent
    │
    │ Download
    ▼
Storage
```

MetroSharp can then monitor the download and update its internal state.

---

# Prowlarr

Prowlarr is used as the indexer/search infrastructure.

Instead of MetroSharp implementing individual integrations for every torrent indexer, Prowlarr provides a centralized interface for indexer management.

Conceptually:

```text
MetroSharp
     │
     ▼
 Prowlarr
     │
 ├── Indexer A
 ├── Indexer B
 ├── Indexer C
 └── ...
```

This keeps MetroSharp independent from individual indexer implementations.

---

# Media Metadata

Movie and series records contain metadata used by the application to describe and organize content.

Metadata may include information such as:

* Title
* Original title
* Release year
* Description
* Poster
* Backdrop
* Genres
* Cast
* Directors
* Ratings
* External IDs
* Runtime
* Release information

External metadata providers can be used to populate this information.

The metadata layer is intentionally separated from the core domain so that external providers can be replaced without rewriting the rest of the application.

---

# Database

MetroSharp uses Entity Framework Core for persistence.

The persistence layer is responsible for:

* Database contexts
* Entity configurations
* Relationships
* Migrations
* Persistence-specific implementations
* Database queries
* Repository implementations where required

The application currently separates database concerns into dedicated contexts where appropriate.

Examples include contexts for:

```text
Application data
Download data
Telegram data
```

The exact database technology can change without requiring the domain model to know about Entity Framework Core.

---

# Background Services

MetroSharp is not purely request/response based.

Several operations need to happen independently of HTTP requests.

For this reason, background services are an important part of the system.

Background processes can be responsible for tasks such as:

* Monitoring downloads
* Processing completed downloads
* Synchronizing media state
* Updating metadata
* Performing scheduled maintenance
* Processing Telegram-related tasks
* Running periodic synchronization jobs

Conceptually:

```text
                    MetroSharp
                        │
             ┌──────────┴──────────┐
             │                     │
         HTTP API             Background Workers
             │                     │
             │          ┌──────────┼──────────┐
             │          ▼          ▼          ▼
             │       Download   Metadata   Processing
             │       Monitor     Sync        Jobs
             │
             ▼
          Database
```

Background services allow long-running or scheduled work to operate independently from API requests.

---

# Project Structure

MetroSharp follows a Clean Architecture-inspired project structure.

```text
/
├── API/
├── Application/
├── Domain/
├── Infrastructure/
├── Persistence/
└── Resource/
```

Each project has a clearly defined responsibility.

---

# API

The `API` project is the entry point of the application.

It contains things such as:

* Controllers
* API configuration
* Dependency injection configuration
* Middleware
* Authentication configuration
* Swagger/OpenAPI configuration
* HTTP-specific behavior
* Application startup

The API project knows about the application layer.

It should not contain core business rules.

Example:

```text
API
 │
 └── Controller
       │
       ▼
   Application
```

---

# Application

The `Application` project contains the application's business use cases.

This is where CQRS is primarily implemented.

Typical contents include:

```text
Application/
├── Commands/
├── Queries/
├── Handlers/
├── DTOs/
├── Behaviors/
├── Interfaces/
└── Services/
```

The application layer answers questions such as:

> What should happen when a manager creates a movie?

or:

> What should happen when a user searches for a movie?

It coordinates the domain and infrastructure without depending on specific infrastructure implementations.

---

# Domain

The `Domain` project contains the core business model.

It is the most independent part of the system.

Typical contents include:

```text
Domain/
├── Entities/
├── ValueObjects/
├── Enums/
├── Exceptions/
├── Interfaces/
└── Domain Rules
```

Examples of domain concepts include:

* Movie
* Series
* Season
* Episode
* User
* Download
* Media file
* Genre

The domain should not depend on:

* ASP.NET Core
* Entity Framework Core
* qBittorrent
* Telegram
* Prowlarr
* HTTP
* Specific databases

The goal is to keep the core business model independent of technology.

---

# Infrastructure

The `Infrastructure` project contains integrations with external systems.

This layer acts as a bridge between MetroSharp and services that exist outside the core application.

Examples include:

```text
Infrastructure/
├── Telegram/
├── QBitTorrent/
├── Prowlarr/
├── Metadata/
├── Media/
└── Other External Services/
```

For example, the application may define an abstraction such as:

```csharp
ITorrentService
```

The infrastructure layer provides the actual implementation:

```text
ITorrentService
       ▲
       │
       │ implements
       │
QBitTorrentService
```

This means the application layer does not need to know how qBittorrent works internally.

It only knows that it can request torrent operations through the abstraction.

---

# Persistence

The `Persistence` project is responsible for database-specific implementation.

It contains:

* Entity Framework Core
* DbContexts
* Entity configurations
* Relationships
* Migrations
* Database implementations

The separation means the domain does not need to know that EF Core exists.

For example:

```text
Domain Entity
     ▲
     │
Persistence Configuration
     │
     ▼
Entity Framework Core
     │
     ▼
Database
```

This makes the persistence mechanism replaceable.

---

# Resource

The `Resource` project contains shared resources used by the application.

Depending on the feature, this may include:

* Localization
* Shared constants
* Embedded resources
* Resource files
* Shared configuration-related resources

Keeping these resources separate prevents them from becoming mixed into the core business logic.

---

# Architecture

MetroSharp follows the dependency rule of Clean Architecture.

The most important concept is:

> Dependencies should point toward the application core.

A simplified dependency graph is:

```text
             ┌──────────────┐
             │     API      │
             └──────┬───────┘
                    │
                    ▼
             ┌──────────────┐
             │ Application  │
             └──────┬───────┘
                    │
             ┌──────┴──────┐
             ▼             ▼
        ┌──────────┐  ┌──────────────┐
        │  Domain  │  │Infrastructure│
        └──────────┘  └──────────────┘
              ▲              │
              │              │
              └──────┬───────┘
                     ▼
                Persistence
```

The exact project references may differ depending on implementation details, but the architectural principle remains the same:

**Core business logic should not become tightly coupled to external technologies.**

---

# CQRS

MetroSharp uses **CQRS — Command Query Responsibility Segregation**.

The basic idea is to separate operations that change state from operations that read state.

## Commands

Commands represent actions.

Examples:

```text
CreateMovie
UpdateMovie
DeleteMovie
StartDownload
CancelDownload
UpdateMovieMetadata
```

A command changes something.

```text
Command
   │
   ▼
Command Handler
   │
   ▼
Domain / Infrastructure
   │
   ▼
State Changed
```

---

## Queries

Queries retrieve information without changing application state.

Examples:

```text
GetMovie
GetMovies
SearchMovies
GetDownloadStatus
GetMovieDetails
GetGenres
```

The flow becomes:

```text
Query
  │
  ▼
Query Handler
  │
  ▼
Database / Read Model
  │
  ▼
Result
```

Separating commands and queries keeps application behavior easier to understand as the system grows.

---

# Why CQRS?

A media management platform naturally contains two very different types of operations.

There can be a large number of reads:

```text
Search movies
List movies
Get movie details
Get genres
Get recommendations
```

while state-changing operations are relatively different:

```text
Create movie
Start download
Update metadata
Delete media
```

CQRS allows these operations to evolve independently.

It also makes individual use cases explicit.

Instead of creating a large service such as:

```text
MovieService
```

with hundreds of methods, the application can organize functionality around individual use cases.

---

# Infrastructure Abstractions

One of the most important architectural decisions in MetroSharp is keeping external services behind abstractions.

For example:

```text
Application
     │
     ▼
ITorrentService
     ▲
     │
     └── QBitTorrentService
```

The same pattern can be used for:

```text
IMetadataService
ITelegramService
IFileService
ISearchService
```

This provides several benefits:

* Easier testing
* Lower coupling
* Easier replacement of external services
* Cleaner application code
* Better separation of responsibilities

---

# Authentication and Authorization

MetroSharp separates public client functionality from privileged management functionality.

Users and managers do not necessarily have the same permissions.

A typical authorization model can be represented as:

```text
                 User
                  │
        ┌─────────┴─────────┐
        ▼                   ▼
 Public Operations      User Operations


                Manager
                   │
                   ▼
          Management Operations
                   │
        ┌──────────┼──────────┐
        ▼          ▼          ▼
     Movies    Downloads    Users
```

Authorization belongs at the application/API boundary rather than being implemented independently inside every client.

This means the same permission model can be used by:

* Mobile clients
* Web clients
* Administrative clients
* Other API consumers

---

# Logging and Monitoring

MetroSharp uses structured application logging to make server operations observable.

Logging is particularly important because MetroSharp contains long-running background processes.

For example, a download can pass through multiple states:

```text
Search started
      ↓
Torrent found
      ↓
Download requested
      ↓
Download started
      ↓
Download progressing
      ↓
Download completed
      ↓
File detected
      ↓
Media processed
      ↓
Movie available
```

Without logging, diagnosing failures in this chain would be difficult.

Logs should therefore provide enough context to determine:

* Which operation was running
* Which media item was involved
* Which external service was contacted
* Whether the operation succeeded
* Why an operation failed

---

# Reverse Proxy and Deployment

MetroSharp is designed to run as a server-side application behind a reverse proxy.

A typical deployment looks like:

```text
                    Internet
                       │
                       ▼
                  Cloudflare
                       │
                       ▼
                     Caddy
                       │
              ┌────────┴────────┐
              │                 │
              ▼                 ▼
        MetroSharp API      Public Media
              │
              ▼
        Internal Services
              │
      ┌───────┼────────┐
      ▼       ▼        ▼
   Database qBit     Prowlarr
```

A reverse proxy provides several useful responsibilities:

* HTTPS termination
* Domain routing
* Reverse proxying
* Static file routing
* Service isolation
* Access control
* Certificate management

The application itself does not need to expose every internal service directly to the internet.

---

# External Services

MetroSharp relies on several external components.

## ASP.NET Core

The main server framework.

Used for:

* REST API
* Dependency injection
* Middleware
* Authentication
* Background services
* HTTP infrastructure

---

## Entity Framework Core

Used for database access and persistence.

---

## qBittorrent

Responsible for torrent downloading.

MetroSharp communicates with qBittorrent rather than implementing torrent functionality itself.

---

## Prowlarr

Responsible for managing and querying torrent/indexer sources.

MetroSharp uses it as the search/discovery layer.

---

## Telegram

Provides the user-facing Telegram client.

MetroSharp integrates with Telegram through the bot infrastructure.

---

## Cloudflare / Caddy

Used as part of the deployment and reverse-proxy infrastructure.

They provide the external networking layer while MetroSharp focuses on application logic.

---

# Typical API Request Flow

Suppose a user wants to retrieve information about a movie.

The flow is:

```text
User
 │
 │ GET /api/movies/{id}
 ▼
API Controller
 │
 │ Query
 ▼
GetMovieQuery
 │
 ▼
GetMovieQueryHandler
 │
 ├──► Persistence
 │
 └──► Domain
 │
 ▼
Movie DTO
 │
 ▼
API Response
 │
 ▼
User
```

The controller itself does not need to understand how the movie is retrieved.

It delegates the operation to the application layer.

---

# Typical Download Flow

A typical download operation is more involved.

```text
Manager/User
     │
     ▼
API
     │
     ▼
StartDownloadCommand
     │
     ▼
Command Handler
     │
     ▼
Search / Select Release
     │
     ▼
Prowlarr
     │
     ▼
Torrent Result
     │
     ▼
qBittorrent
     │
     ▼
Torrent Download
     │
     ▼
Storage
     │
     ▼
Background Worker
     │
     ▼
Media Processing
     │
     ▼
Database Updated
     │
     ▼
Movie Available
```

This is one of the central workflows of MetroSharp.

---

# Separation Between Metadata and Media

MetroSharp intentionally treats metadata and actual media files as separate concepts.

For example:

```text
Movie
 ├── Metadata
 │    ├── Title
 │    ├── Description
 │    ├── Poster
 │    ├── Cast
 │    └── Genres
 │
 └── Media
      ├── File
      ├── Size
      ├── Quality
      └── Availability
```

This allows a movie to exist in the catalog before its media is available.

It also allows metadata to be updated without touching the media file itself.

---

# Scalability Considerations

MetroSharp is designed around independent components so individual parts can evolve independently.

For example:

```text
API
 │
 ├── Database
 │
 ├── Download Services
 │
 ├── Telegram
 │
 └── Background Workers
```

If the movie library grows from:

```text
20,000
```

to:

```text
100,000+
```

the application can evolve without requiring the entire architecture to be rewritten.

Potential future improvements include:

* Dedicated worker processes
* Distributed job queues
* Caching
* Read-optimized database models
* Dedicated search infrastructure
* Object storage
* CDN integration
* Separate media-processing workers

The current architecture provides a foundation for these changes without requiring clients to change their API contract.

---

# Design Goals

MetroSharp was designed around several principles.

## 1. Separation of concerns

Each component should have one primary responsibility.

```text
API             → HTTP
Application     → Use cases
Domain          → Business rules
Persistence     → Database
Infrastructure  → External systems
```

---

## 2. Replaceability

External technologies should not become part of the application's core business logic.

For example:

```text
Application
     │
     ▼
ITorrentService
     ▲
     │
qBittorrent implementation
```

If the torrent implementation changes, the application layer should not need to be rewritten.

---

## 3. Multiple clients

The backend should not depend on a particular client.

The same API can serve:

* Mobile
* Web
* Telegram
* Future desktop clients
* Future third-party clients

---

## 4. Automation

Media management involves many operations that should not require manual intervention.

Background services are therefore a core part of the architecture.

---

## 5. Maintainability

The project is divided into independent projects and features so that new functionality can be added without turning the codebase into a monolithic collection of unrelated services.

---

# Why the Architecture Is Structured This Way

A project like MetroSharp can become complicated very quickly.

It combines:

* HTTP
* Databases
* Authentication
* Media
* Torrent downloads
* External APIs
* Telegram
* Background processing
* File systems
* Metadata
* Multiple clients

Putting all of this into a single ASP.NET Core project would make the system increasingly difficult to maintain.

Instead, MetroSharp separates these responsibilities.

For example:

```text
                     MetroSharp
                          │
        ┌─────────────────┼─────────────────┐
        │                 │                 │
        ▼                 ▼                 ▼
      API            Application         Workers
        │                 │                 │
        │                 ▼                 │
        │              Domain               │
        │                 │                 │
        └──────────┬──────┴──────┬──────────┘
                   │             │
                   ▼             ▼
              Persistence   Infrastructure
                                 │
                    ┌────────────┼────────────┐
                    ▼            ▼            ▼
               qBittorrent    Prowlarr    Telegram
```

Each layer can evolve without forcing every other layer to change.

---

# Project Philosophy

MetroSharp is not intended to be an enormous enterprise platform.

The goal is to maintain a practical architecture that is:

* Structured
* Understandable
* Extensible
* Testable
* Maintainable
* Suitable for a large personal/private media platform

The architecture provides enough separation to prevent the application from becoming tightly coupled while avoiding unnecessary abstraction for its own sake.

---

# Future Development

Possible future improvements include:

### Media processing

* Better automatic file matching
* Automatic quality detection
* Subtitle processing
* Media validation
* Automatic organization

### Search

* Faster full-text search
* Dedicated search indexes
* Advanced filtering
* Better ranking

### Download management

* Smarter torrent selection
* Automatic retry
* Download prioritization
* Health monitoring
* Failed-download recovery

### Background processing

* Dedicated worker services
* Distributed queues
* Scheduled jobs
* Better retry mechanisms

### Client applications

* Improved mobile application
* Web frontend
* Desktop application
* Expanded Telegram functionality

### Infrastructure

* CDN integration
* Distributed storage
* Object storage
* Horizontal API scaling
* Dedicated media-processing nodes

---

# Technology Stack

| Component           | Technology                   |
| ------------------- | ---------------------------- |
| Backend             | ASP.NET Core 9               |
| Language            | C#                           |
| Architecture        | Clean Architecture           |
| Application Pattern | CQRS                         |
| Database Access     | Entity Framework Core        |
| Database            | SQLite / relational database |
| Logging             | Serilog                      |
| Torrent Client      | qBittorrent                  |
| Indexer Management  | Prowlarr                     |
| Messaging / Client  | Telegram Bot                 |
| Reverse Proxy       | Caddy                        |
| CDN / DNS / Edge    | Cloudflare                   |
| Mobile Client       | React Native / Expo          |
| API Style           | REST                         |

---

# Repository Structure

A simplified view of the repository:

```text
MetroSharp/
│
├── API/
│   ├── Controllers/
│   ├── Middleware/
│   ├── Configuration/
│   └── Program.cs
│
├── Application/
│   ├── Commands/
│   ├── Queries/
│   ├── Handlers/
│   ├── DTOs/
│   ├── Behaviors/
│   └── Interfaces/
│
├── Domain/
│   ├── Entities/
│   ├── Enums/
│   ├── ValueObjects/
│   ├── Exceptions/
│   └── Interfaces/
│
├── Infrastructure/
│   ├── Telegram/
│   ├── QBitTorrent/
│   ├── Prowlarr/
│   ├── Metadata/
│   ├── Media/
│   └── Services/
│
├── Persistence/
│   ├── Contexts/
│   ├── Configurations/
│   ├── Migrations/
│   └── Repositories/
│
└── Resource/
    ├── Localization/
    └── Resources/
```

The actual repository structure may contain additional projects and folders as the system evolves.

---

# Summary

MetroSharp is the backend infrastructure that powers Metro Moviez.

It combines a REST API, database, background workers, media processing, torrent management, indexer integration, and Telegram functionality into a single coordinated platform.

Its central architectural principle is separation of responsibilities:

```text
                    ┌───────────────┐
                    │    Clients    │
                    └───────┬───────┘
                            │
                            ▼
                    ┌───────────────┐
                    │      API      │
                    └───────┬───────┘
                            │
                            ▼
                    ┌───────────────┐
                    │  Application  │
                    │    + CQRS     │
                    └───────┬───────┘
                            │
                            ▼
                    ┌───────────────┐
                    │    Domain     │
                    └───────┬───────┘
                            │
              ┌─────────────┴─────────────┐
              ▼                           ▼
       ┌──────────────┐           ┌──────────────┐
       │ Persistence  │           │Infrastructure│
       └──────┬───────┘           └──────┬───────┘
              │                          │
              ▼                    ┌─────┼─────┐
          Database                 ▼     ▼     ▼
                               qBit  Prowlarr Telegram
```
