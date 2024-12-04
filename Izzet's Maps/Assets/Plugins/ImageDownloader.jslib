mergeInto(LibraryManager.library, {
    DownloadImage: function(base64DataPtr, length, fileNamePtr) {
        // Convert pointers to strings
        var base64Data = UTF8ToString(base64DataPtr, length);
        var fileName = UTF8ToString(fileNamePtr);

        // Create a data URL
        var link = document.createElement('a');
        link.href = 'data:image/png;base64,' + base64Data;
        link.download = fileName;
        link.click();
    }
});