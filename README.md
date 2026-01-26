# VODPipeline-API
A tool for automatically ingesting and editing VOD content.

## Architecture

This API is part of a composite application for VOD (Video on Demand) content processing. The complete system consists of:

- **VODPipeline-API** (this repository) - Backend API service that handles video ingestion, processing, and management
- **[VODPipeline-UI](https://github.com/loganmj/VODPipeline-UI)** - Frontend user interface for interacting with the VOD pipeline
- **[VODPipeline-Function](https://github.com/loganmj/VODPipeline-Function)** - Serverless functions for background video processing tasks

## Development

This project uses .NET 10.0. To build the solution:

```bash
dotnet build VodDashboard.sln
```

### Pull Request Requirements

All pull requests targeting the `main` branch must pass the automated build check before merging.
