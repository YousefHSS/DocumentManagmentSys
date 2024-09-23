// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function copyHeaderWidths(sourceTable, targetTable) {
    const sourceHeaders = sourceTable.querySelectorAll('th');
    const targetHeaders = targetTable.querySelectorAll('th');
    var z = 0;
    if (sourceHeaders.length !== targetHeaders.length) {
        console.error('Tables have different number of headers');
        return;
    }

    for (let i = 0; i < sourceHeaders.length; i++) {
        const sourceWidth = window.getComputedStyle(sourceHeaders[i]).getPropertyValue('width');
        targetHeaders[i].style.width = sourceWidth;
        targetHeaders[i].style.zIndex = z++;
        
    }
}

//on document ready
document.addEventListener("DOMContentLoaded", function (event) {
    const sourceTable = document.getElementById('sourceTable');
    const targetTable = document.getElementById('targetTable');

    if (sourceTable && targetTable) {
        copyHeaderWidths(sourceTable, targetTable);

    }
    ApplyUrlParams();
    

    const form = document.getElementById('searchForm'); // Replace with your form's ID
    if (form) {
        form.addEventListener('submit', (event) => {
            event.preventDefault(); // This stops the form from submitting
            // Your code here to handle the form data
            console.log('Form submission intercepted');

            ApplyFilters();

            // If you want to submit the form programmatically later:
             form.submit();
        });
    }

   
});

function ApplyUrlParams() {
    const urlParams = new URLSearchParams(window.location.search);
    ClearFilters();
    const searchForm = document.getElementById('searchForm');



    if (!searchForm) {
        console.error('searchForm is not found in the DOM');
        return;
    }
    if (urlParams.has('DN') && urlParams.get("DN") != '') {
        document.querySelector('input[name="Document Name"][value=' + urlParams.get("DN") + ']').click();
    }
    if (urlParams.has('VR') && urlParams.get("VR") != '') {
        document.querySelector('input[name="Version"][value=' + urlParams.get("VR") + ']').click();
    
    }
    if (urlParams.has('CA') && urlParams.get("CA") != '') {
        document.querySelector('input[name="Created At"][value=' + urlParams.get("CA") + ']').click();
    }
    if (urlParams.has('UA') && urlParams.get("UA") != '') {
        document.querySelector('input[name="Updated At"][value=' + urlParams.get("UA") + ']').click();
    }

    if (urlParams.has('UP') && urlParams.get("UP") != '') {
        document.querySelector('input[name="Updated"]').click();
    }

    if (urlParams.has('DD') && urlParams.get("DD") != '') {
        document.querySelector('input[name="Downloaded"]').click();
    }

  
    if (urlParams.has('SS') && urlParams.get("SS") != '') {
        //example 1%2CApproved%2CRejected%2CUnder_Revison%2CUnder_Finalization
        const encodedString = urlParams.get('SS');
        const decodedString = decodeURIComponent(encodedString);
        const array = decodedString.split(',');
        CheckStatuses(
            array
        );

    }
    if (urlParams.has('search')) {
        document.querySelector('input[name="search"]').value = urlParams.get('search');
    }


   
}

function CheckStatuses(Statuses) {
    var msList = waitForElement('.multiselect-dropdown-list', 30000);
    var Allflag = false;
    let label;
    msList.then(function () {
        //click on the child that is the input whith a sibling that is the label that has the text of the status
        console.log(Statuses)
        Statuses.forEach(status => {
            
            if (status == '1') {
                Allflag = true;
                label = Array.from(document.querySelectorAll('label')).find(lbl => lbl.textContent === 'All');

            } else {
                // Find the label that contains the text of the status
                label = Array.from(document.querySelectorAll('label')).find(lbl => lbl.textContent === status.replace('_',' '));
            }
            
            // If the label is found, find the associated input and check it
            if (label) {
                let input = label.previousElementSibling;
                if (input && input.type === 'checkbox') {
                   
                    if (status != "1") {
                        input.click();
                    }
                        
                        
                    
                        
                    
                    
                }
            }
        });
        if (!Allflag) {
            const label2 = Array.from(document.querySelectorAll('label')).find(lbl => lbl.textContent === 'All');
            const input2 = label2.previousElementSibling;

            input2.click();

        }
    });
   
   
}
function toggleTransition() {
    fillSearchFormData()
    const element = document.getElementById('targetTable');

    const TR = document.getElementById('HH');
    if (!TR.classList.contains('ShowHightTR')) {

        TR.classList.remove('d-none');
    }
    //get computed hight
    
    element.classList.toggle('transitionTBShow');
    TR.classList.toggle('ShowHightTR');
    var toggle = element.classList.contains('tesTR');
    if (!TR.classList.contains('ShowHightTR')) {

        //time out 0.6 sec
        setTimeout(function () {
            TR.classList.add('d-none');
        }, 600); 
       
        
    }
   
    
}
function GetNamesOfChangedValues(){
    const searchForm = document.getElementById('searchForm');
    if (!searchForm) {
        console.error('searchForm is not found in the DOM');
        return [];
    }

    const changedNames=[];

    // Use the HTMLFormElement interface to iterate over elements
    if (searchForm instanceof HTMLFormElement) {
        for (const element of searchForm.elements) {
            // Ensure element is an instance of HTMLElement to access its properties
            if (element instanceof HTMLElement) {
                const name = element.id; // Fallback to ID if name is not available
                const currentElement = document.querySelector(`[name="${name}"]`);
                
                // Check if currentElement is valid and has a value property
                if (currentElement && 'value' in currentElement) {
                    var originalValue;
                    var currentValue;
                        if (currentElement.type === 'radio' || currentElement.type === 'checkbox') {
                            var newelement = document.querySelector(`[name="${name}"]:checked`);
                             originalValue = (element).value;
                            currentValue = newelement===null?'':(newelement).value;
                            if (originalValue !== currentValue) {
                                changedNames.push(name);
                            }
                        }
                        else if (currentElement.type === 'select-multiple') {
                            var newelement = document.querySelector(`[name="${name}"]`);
                            //currentElement.value  if not an array create an array
                            var test;
                            if (!Array.isArray(currentElement.value)) {
                                test = Array.from([currentElement.value]);

                            }

                            originalValue = test
                            currentValue = newelement === null ? '' : Array.from(newelement.selectedOptions).map(option => option.value)
       
                            if (JSON.stringify(originalValue) !== JSON.stringify(currentValue)) {
                                changedNames.push(name);
                            }
                        }
                        else {
                             originalValue = (element).value;
                            currentValue = (currentElement).value;
                            // Compare values for inputs, selects, and textareas
                            if (originalValue !== currentValue) {
                                changedNames.push(name);
                            }
                        }
                   
                } 
            }
        }
    }
    //console.log(changedNames);
    return changedNames;
}

function fillSearchFormData() {
    //get search form
    const searchForm = document.getElementById('searchForm');
    //get data from the tab
    //first the radio buttons data
    const DocumentNameRadioButton = document.querySelector('input[type="radio"][name="Document Name"]:checked');
    const VersionRadioButton = document.querySelector('input[type="radio"][name="Version"]:checked');
    const CreatedAtRadioButton = document.querySelector('input[type="radio"][name="Created At"]:checked');
    const UpdatedAtRadioButton = document.querySelector('input[type="radio"][name="Updated At"]:checked');

    //then the multiselect data
    const multiSelects = document.getElementsByName('Statuses');
    const multiSelect = multiSelects[0];
    const selectedValues = Array.from(multiSelect.options)
        .filter(option => option.selected)
        .map(option => option.value);

    //now the Check boxes data

    const UpdatedcheckBox = document.querySelector('input[type="checkbox"][name="Updated"]:checked');
    const DownloadedcheckBox = document.querySelector('input[type="checkbox"][name="Downloaded"]:checked');

    //now fill the form
    searchForm.querySelector('input[id="Document Name"]').value = DocumentNameRadioButton==null?'':DocumentNameRadioButton.value ;
    searchForm.querySelector('input[id="Version"]').value = VersionRadioButton == null ? '' : VersionRadioButton.value;
    searchForm.querySelector('input[id="Created At"]').value = CreatedAtRadioButton == null ? '' : CreatedAtRadioButton.value;
    searchForm.querySelector('input[id="Updated At"]').value = UpdatedAtRadioButton == null ? '' : UpdatedAtRadioButton.value;
    searchForm.querySelector('input[id="Statuses"]').value = selectedValues == null ? '' : selectedValues.join(',');
    searchForm.querySelector('input[id="Updated"]').value = UpdatedcheckBox == null ? '' : UpdatedcheckBox.value;
    searchForm.querySelector('input[id="Downloaded"]').value = DownloadedcheckBox == null ? '' : DownloadedcheckBox.value;
    ////log the data
    //console.log("Document Name:", DocumentNameRadioButton);
    //console.log("Version:", VersionRadioButton);
    //console.log("Created At:", CreatedAtRadioButton);
    //console.log("Updated At:", UpdatedAtRadioButton);
    //console.log("Statuses:", selectedValues);
    //console.log("Updated:", UpdatedcheckBox);
    //console.log("Downloaded:", DownloadedcheckBox);

    
    






}

function lightbox(element) {
    /*get other check boxes with same name*/
    const newelements = document.getElementsByName(element.name);
    newelements.forEach(elementt => {
       
        elementt.parentElement.classList.remove('lightbox');
    });
    element.parentElement.classList.add('lightbox');
}

function ActivateClearAndApply() {
    if (GetNamesOfChangedValues().length > 0) {
        //elemnts with box class
        document.querySelectorAll('.filterbtn').forEach(element => {
            element.style.display='flex';
        });

    }
}

function DeactivateClearButtonAndApply() {
    document.querySelectorAll('.filterbtn').forEach(element => {
        element.style.display = 'none';
        //make it important
        element.classList.remove('d-flex');

    });
    document.querySelectorAll('.filterbtn')[0].style.display = 'flex';

}


function ClearFilters(){
    //get data from the tab
    //first the radio buttons data
    const DocumentNameRadioButton = document.querySelectorAll('input[type="radio"][name="Document Name"]');
    const VersionRadioButton = document.querySelectorAll('input[type="radio"][name="Version"]');
    const CreatedAtRadioButton = document.querySelectorAll('input[type="radio"][name="Created At"]');
    const UpdatedAtRadioButton = document.querySelectorAll('input[type="radio"][name="Updated At"]');

    //then the multiselect data
    const multiSelects = document.getElementsByName('Statuses');
    const multiSelect = multiSelects[0];
    const selectedValues = Array.from(multiSelect.options)
        .filter(option => option.selected)
        .map(option => option.value);

    //now the Check boxes data

    const UpdatedcheckBox = document.querySelector('input[type="checkbox"][name="Updated"]');
    const DownloadedcheckBox = document.querySelector('input[type="checkbox"][name="Downloaded"]');
    
    //now clear everything
    DocumentNameRadioButton.forEach(rb => {
        rb.checked = false;
        lightbox(rb);
    });
    VersionRadioButton.forEach(rb => {
        rb.checked = false;
        lightbox(rb);
    });
    CreatedAtRadioButton.forEach(rb => {
        rb.checked = false;
        lightbox(rb);
    });
    UpdatedAtRadioButton.forEach(rb => {
        rb.checked = false;
        lightbox(rb);
    });

    UpdatedcheckBox.checked = false;
    DownloadedcheckBox.checked = false;
    multiSelect.value = ['1'];

    DeactivateClearButtonAndApply();

    

}

function ApplyFilters() {
    fillSearchFormData();
    const searchForm = document.getElementById('searchForm');
    searchForm.submit();
}

function BlockClicked(element) {
    //click on the child that is the input
    console.log(element);
    var input = element.querySelector('input');
    input.click();
}


function waitForElement(selector, timeout = 30000) {
    return new Promise((resolve, reject) => {
        const element = document.querySelector(selector);
        if (element) {
            resolve(element);
            return;
        }

        const observer = new MutationObserver(mutations => {
            mutations.forEach(mutation => {
                if (mutation.addedNodes) {
                    const elements = Array.from(mutation.addedNodes).filter(node => {
                        return node instanceof HTMLElement && node.matches(selector);
                    });
                    if (elements.length > 0) {
                        observer.disconnect();
                        resolve(elements[0]);
                    }
                }
            });
        });

        observer.observe(document.documentElement, {
            childList: true,
            subtree: true
        });

        // Optional: reject the promise after a timeout
        const timeoutId = setTimeout(() => {
            observer.disconnect();
            reject(new Error(`Timeout waiting for element ${selector}`));
        }, timeout);

        // Cleanup the observer and timeout when the promise settles
        const cleanup = () => {
            clearTimeout(timeoutId);
            observer.disconnect();
        };

        // Attach cleanup to promise resolution and rejection
        const promise = Promise.race([]);
        promise.then(cleanup, cleanup);
    });
}

