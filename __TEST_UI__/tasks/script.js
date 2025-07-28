document.addEventListener('DOMContentLoaded', () => {
    // --- Configuration ---
    const API_BASE_URL = 'http://localhost:5159/api'; // IMPORTANT: Update with your API's URL

    // --- DOM Elements ---
    const loginContainer = document.getElementById('login-container');
    const taskContainer = document.getElementById('task-container');
    const loginForm = document.getElementById('login-form');
    const emailInput = document.getElementById('email');
    const passwordInput = document.getElementById('password');
    const loginError = document.getElementById('login-error');
    const logoutBtn = document.getElementById('logout-btn');
    const taskTableBody = document.getElementById('task-table-body');
    const loadingIndicator = document.getElementById('loading');
    
    // Filters
    const titleFilter = document.getElementById('title-filter');
    const stateFilter = document.getElementById('state-filter');
    const filterBtn = document.getElementById('filter-btn');

    // Pagination
    const prevPageBtn = document.getElementById('prev-page');
    const nextPageBtn = document.getElementById('next-page');
    const pageInfo = document.getElementById('page-info');

    // Modal
    const modal = document.getElementById('task-modal');
    const modalBody = document.getElementById('modal-body');
    const closeModalBtn = document.querySelector('.close-button');

    // --- State Management ---
    let currentPage = 1;
    let totalPages = 1;
    const pageSize = 10;
    const taskStates = [
        "Pending", "JobQueue", "JobStarted", "InGettingOmdbDetailsProcess", 
        "InQbitButDownloadNotStarted", "TorrentTimedOut", "InQbitAndDownloadStarted", 
        "TorrentWasDownloaded", "InDownloadingSubtitle", "InParingSubtitlesWithVideo", 
        "InFfmpegButProcessNotStarted", "InFfmpegAndProcessStarted", 
        "InUploaderButUploadingNotStarted", "InUploaderAndUploadingStarted", 
        "Completed", "Error"
    ];

    // --- Initialization ---
    function init() {
        populateStateFilter();
        const token = localStorage.getItem('authToken');
        if (token) {
            showTaskViewer();
        } else {
            showLogin();
        }

        loginForm.addEventListener('submit', handleLogin);
        logoutBtn.addEventListener('click', handleLogout);
        filterBtn.addEventListener('click', () => {
            currentPage = 1;
            fetchTasks();
        });
        prevPageBtn.addEventListener('click', () => {
            if (currentPage > 1) {
                currentPage--;
                fetchTasks();
            }
        });
        nextPageBtn.addEventListener('click', () => {
            if (currentPage < totalPages) {
                currentPage++;
                fetchTasks();
            }
        });
        closeModalBtn.addEventListener('click', () => modal.classList.add('hidden'));
        window.addEventListener('click', (event) => {
            if (event.target === modal) {
                modal.classList.add('hidden');
            }
        });
    }
    
    function populateStateFilter() {
        taskStates.forEach(state => {
            const option = document.createElement('option');
            option.value = state;
            option.textContent = state;
            stateFilter.appendChild(option);
        });
    }

    // --- UI Control ---
    function showLogin() {
        loginContainer.classList.remove('hidden');
        taskContainer.classList.add('hidden');
    }

    function showTaskViewer() {
        loginContainer.classList.add('hidden');
        taskContainer.classList.remove('hidden');
        fetchTasks();
    }

    function showLoading(isLoading) {
        if (isLoading) {
            loadingIndicator.classList.remove('hidden');
            taskTableBody.innerHTML = '';
        } else {
            loadingIndicator.classList.add('hidden');
        }
    }

    // --- API Calls ---
    async function handleLogin(event) {
        event.preventDefault();
        loginError.textContent = '';
        const email = emailInput.value;
        const password = passwordInput.value;

        try {
            const response = await fetch(`${API_BASE_URL}/account/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password })
            });

            const result = await response.json();

            if (!response.ok || result.status !== 'success') {
                throw new Error(result.messages ? result.messages.join(', ') : 'Login failed');
            }

            localStorage.setItem('authToken', result.data.token);
            showTaskViewer();

        } catch (error) {
            loginError.textContent = error.message;
        }
    }

    function handleLogout() {
        localStorage.removeItem('authToken');
        showLogin();
    }

    async function fetchTasks() {
        const token = localStorage.getItem('authToken');
        if (!token) {
            handleLogout();
            return;
        }

        showLoading(true);

        const params = new URLSearchParams({
            pageNumber: currentPage,
            pageSize: pageSize
        });

        if (titleFilter.value) params.append('title', titleFilter.value);
        if (stateFilter.value) params.append('state', stateFilter.value);

        try {
            const response = await fetch(`${API_BASE_URL}/tasks/query-tasks?${params.toString()}`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });

            if (response.status === 401) {
                handleLogout();
                return;
            }

            const result = await response.json();
            if (!response.ok || result.status !== 'success') {
                throw new Error(result.messages ? result.messages.join(', ') : 'Failed to fetch tasks');
            }
            
            renderTasks(result.data.items);
            updatePagination(result.data);

        } catch (error) {
            console.error('Error fetching tasks:', error);
            taskTableBody.innerHTML = `<tr><td colspan="6" class="error-message">${error.message}</td></tr>`;
        } finally {
            showLoading(false);
        }
    }

    async function fetchTaskDetails(taskId) {
        const token = localStorage.getItem('authToken');
        if (!token) return;

        try {
            const response = await fetch(`${API_BASE_URL}/tasks/${taskId}`, {
                 headers: { 'Authorization': `Bearer ${token}` }
            });
             if (response.status === 401) {
                handleLogout();
                return;
            }
            const result = await response.json();
            if (!response.ok || result.status !== 'success') {
                throw new Error('Failed to fetch task details');
            }
            return result.data;
        } catch (error) {
            console.error('Error fetching task details:', error);
            return null;
        }
    }

    // --- Rendering ---
    function renderTasks(tasks) {
        taskTableBody.innerHTML = '';
        if (tasks.length === 0) {
            taskTableBody.innerHTML = '<tr><td colspan="6">No tasks found.</td></tr>';
            return;
        }

        tasks.forEach(task => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${task.title || 'N/A'}</td>
                <td><span class="status status-${task.state || 'Default'}">${task.state || 'Unknown'}</span></td>
                <td>${task.userName || 'N/A'}</td>
                <td>${task.downloadProgress.toFixed(1)}%</td>
                <td>${new Date(task.createdAt).toLocaleString()}</td>
                <td><button class="btn btn-secondary view-details-btn" data-id="${task.id}">Details</button></td>
            `;
            taskTableBody.appendChild(row);
        });
        
        document.querySelectorAll('.view-details-btn').forEach(button => {
            button.addEventListener('click', async (e) => {
                const taskId = e.target.dataset.id;
                const details = await fetchTaskDetails(taskId);
                if (details) {
                    renderModal(details);
                }
            });
        });
    }
    
    function renderModal(task) {
        modalBody.innerHTML = `
            <p><strong>ID:</strong> ${task.id}</p>
            <p><strong>Title:</strong> ${task.title}</p>
            <p><strong>Torrent Title:</strong> ${task.torrentTitle || 'N/A'}</p>
            <p><strong>Status:</strong> ${task.state}</p>
            <p><strong>IMDb ID:</strong> ${task.imdbId}</p>
            <p><strong>User:</strong> ${task.userName}</p>
            <p><strong>Download:</strong> ${task.downloadProgress.toFixed(2)}% at ${ (task.downloadSpeed / 1024).toFixed(2)} KB/s</p>
            <p><strong>Created:</strong> ${new Date(task.createdAt).toLocaleString()}</p>
            <p><strong>Last Updated:</strong> ${new Date(task.updatedAt).toLocaleString()}</p>
             ${task.errorMessage ? `<p><strong>Error:</strong> <span class="error-message">${task.errorMessage}</span></p>` : ''}
        `;
        modal.classList.remove('hidden');
    }

    function updatePagination(data) {
        currentPage = data.currentPage;
        totalPages = data.totalPages;

        pageInfo.textContent = `Page ${currentPage} of ${totalPages}`;
        prevPageBtn.disabled = currentPage <= 1;
        nextPageBtn.disabled = currentPage >= totalPages;
    }

    // --- Start the App ---
    init();
});
