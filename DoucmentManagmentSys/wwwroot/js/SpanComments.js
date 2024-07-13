// Specify the PDF file URL
const pdfFile = '../TempView/Validation Protocol of Assay(1).pdf';

// Asynchronously load and render the PDF file
pdfjsLib.getDocument(pdfFile).promise.then(pdfDoc => {
    // Get the first page of the PDF
    pdfDoc.getPage(1).then(page => {
        const scale = 1.5;
        const viewport = page.getViewport({ scale });

        // Prepare canvas using PDF page dimensions
        const canvas = document.createElement('canvas');
        const context = canvas.getContext('2d');
        canvas.height = viewport.height;
        canvas.width = viewport.width;

        // Render PDF page into canvas context
        const renderContext = {
            canvasContext: context,
            viewport: viewport
        };
        page.render(renderContext).promise.then(() => {
            // Convert the rendered canvas to HTML
            const html = canvas.toDataURL('image/png');

            // Return the resulting HTML
            console.log(html); // or process the HTML as needed
        });
    });
});