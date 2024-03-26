
# Accessing package documentation

Because this package is under NDA, Unity provides the HTML documentation locally, in the `HTMLDocs~` folder. You must run a local webserver to view the HTML files correctly.

## Viewing the documentation in a web browser

The following method of running a local webserver requires Python 3 to be installed.

Run the following command in a command prompt in the same location as you Unity project folder.

```
python -m http.server
```

You will see output similar to this.

```
Serving HTTP on :: port 8000 (http://[::]:8000/) ...
```

Open a web browser and enter the URL below. Use the port number reported in the output.

```
http://localhost:8000/
```

This shows a directory listing from the directory where webserver is running.

Navigate to the `HTLMDocs~` folder in the package to open the HTML documentation for the package.

