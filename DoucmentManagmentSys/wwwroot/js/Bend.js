function positionElementsInCircle(selector, radius) {
    var circle = document.querySelector(selector);
    circle.style.width = radius * 2 + 'px';
    circle.style.height = radius * 2 + 'px';
    const elements = circle.children;
    const numElements = elements.length;
    const angleStep = 360 / numElements;

    Array.from(elements).forEach((el, index) => {
        const angle = angleStep * index;
        const x = radius * Math.cos(angle * Math.PI / 180);
        const y = radius * Math.sin(angle * Math.PI / 180);
        // Assuming the circle div is positioned relatively or absolutely in its container
        el.style.position = 'absolute'; // Make sure elements are positioned absolutely within the circle
        el.style.left = x + 'px';
        el.style.top = y + 'px';
        createCurvedTextWithin(el, angle, radius); // Pass the angle for correct letter orientation
    });
}

function createCurvedTextWithin(element, angle , radius) {
    const text = element.innerText;
    element.innerHTML = ''; // Clear the original text

    const degreeIncrement = 360 / text.length; // Adjust based on the desired curvature
    let currentDegree = 0;

    for (let char of text) {
        const characterPosition = document.createElement('div');
        characterPosition.style.display = 'inline-block';
        characterPosition.style.transformOrigin = '50% 50%';
        const characterRotation = document.createElement('div');
        characterRotation.innerText = char;
        // Adjust rotation based on the angle to face outwards
        characterRotation.style.transform = `rotate(${angle + currentDegree}deg)`;
        characterPosition.appendChild(characterRotation);

        element.appendChild(characterPosition);
        currentDegree += degreeIncrement;
    }
}
function injectHTMLIntoIframe(iframeId, htmlContent) {
    const iframe= document.getElementById(iframeId);

    if (iframe && iframe.contentWindow && iframe.contentWindow.document) {
        // Open the iframe document for writing
        iframe.contentWindow.document.open();
        iframe.contentWindow.document.write(htmlContent);
        iframe.contentWindow.document.close();
    } else {
        console.error('Iframe not found or not accessible');
    }
}

//positionElementsInCircle('.circle', 100); // Adjust '100' based on the desired radius