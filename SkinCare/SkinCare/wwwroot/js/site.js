// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Predictive Search Functionality
document.addEventListener('DOMContentLoaded', () => {
    const searchInput = document.getElementById('searchInput');
    const searchSuggestions = document.getElementById('searchSuggestions');
    const searchForm = document.getElementById('searchForm');
    
    if (!searchInput || !searchSuggestions || !searchForm) return;
    
    let debounceTimer;
    let currentFocus = -1;
    let suggestions = [];
    
    // Debounce function to limit API calls
    const debounce = (callback, delay = 300) => {
        return function() {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => {
                callback.apply(this, arguments);
            }, delay);
        };
    };
    
    // Fetch suggestions from API
    const fetchSuggestions = debounce(async (query) => {
        if (query.length < 2) {
            searchSuggestions.innerHTML = '';
            searchSuggestions.style.display = 'none';
            return;
        }
        
        try {
            const response = await fetch(`/Products/GetSearchSuggestions?query=${encodeURIComponent(query)}`);
            if (!response.ok) throw new Error('Network response was not ok');
            
            suggestions = await response.json();
            
            // Display suggestions
            displaySuggestions(suggestions);
        } catch (error) {
            console.error('Error fetching search suggestions:', error);
            searchSuggestions.innerHTML = '';
            searchSuggestions.style.display = 'none';
        }
    }, 300);
    
    // Display suggestions in dropdown
    const displaySuggestions = (items) => {
        searchSuggestions.innerHTML = '';
        
        if (items.length === 0) {
            searchSuggestions.style.display = 'none';
            return;
        }
        
        items.forEach(item => {
            const div = document.createElement('div');
            div.textContent = item;
            div.className = 'search-suggestion-item';
            div.addEventListener('click', () => {
                searchInput.value = item;
                searchSuggestions.style.display = 'none';
                searchForm.submit();
            });
            searchSuggestions.appendChild(div);
        });
        
        searchSuggestions.style.display = 'block';
        currentFocus = -1;
    };
    
    // Handle input changes
    searchInput.addEventListener('input', (e) => {
        const query = e.target.value.trim();
        fetchSuggestions(query);
    });
    
    // Handle keyboard navigation
    searchInput.addEventListener('keydown', (e) => {
        const items = searchSuggestions.getElementsByClassName('search-suggestion-item');
        if (!items.length) return;
        
        // Arrow down
        if (e.key === 'ArrowDown') {
            currentFocus++;
            addActive(items);
            e.preventDefault();
        } 
        // Arrow up
        else if (e.key === 'ArrowUp') {
            currentFocus--;
            addActive(items);
            e.preventDefault();
        } 
        // Enter
        else if (e.key === 'Enter') {
            if (currentFocus > -1) {
                if (items[currentFocus]) {
                    items[currentFocus].click();
                    e.preventDefault();
                }
            }
        }
    });
    
    // Add active class to current focus item
    const addActive = (items) => {
        if (!items) return;
        
        removeActive(items);
        
        if (currentFocus >= items.length) currentFocus = 0;
        if (currentFocus < 0) currentFocus = items.length - 1;
        
        items[currentFocus].classList.add('active');
        searchInput.value = items[currentFocus].textContent;
    };
    
    // Remove active class from all items
    const removeActive = (items) => {
        for (let i = 0; i < items.length; i++) {
            items[i].classList.remove('active');
        }
    };
    
    // Close dropdown when clicking outside
    document.addEventListener('click', (e) => {
        if (e.target !== searchInput && e.target !== searchSuggestions) {
            searchSuggestions.style.display = 'none';
        }
    });
    
    // Focus input when clicking on search form
    searchForm.addEventListener('click', () => {
        searchInput.focus();
    });
});
