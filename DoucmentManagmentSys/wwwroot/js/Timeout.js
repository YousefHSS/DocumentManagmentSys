// Function to redirect to the login page
function logout() {

        button = document.getElementById('logout');
        button.click();

}


// Function to reset the inactivity timer
function resetTimer() {
    
    clearTimeout(inactivityTimeout);
    inactivityTimeout = setTimeout(logout, 5 * 60 * 1000); // 5 minutes in milliseconds
}

// Set the initial inactivity timer
let inactivityTimeout = setTimeout(logout, 5 * 60 * 1000);

// Reset the timer on any of these events
['mousemove', 'keydown', 'mousedown', 'touchstart'].forEach(event => {
    
    window.addEventListener(event, resetTimer);
});

