// Chat functionality with SignalR
document.addEventListener('DOMContentLoaded', () => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/chatHub')
        .build();

    // Set up SignalR connection
    connection.start().catch(err => console.error(err.toString()));

    // Listen for new messages
    connection.on("ReceiveMessage", (message) => {
        // Update unread count when new message is received
        loadUnreadBadge();
    });

    // Listen for unread count updates
    connection.on("UnreadCountUpdated", () => {
        loadUnreadBadge();
    });

    // Load unread count on page load
    loadUnreadBadge();
    
    // Mark messages as read when chat opens
    document.addEventListener('click', async function(event) {
        if (event.target.closest('.chat-contact, .conversation-row')) {
            const element = event.target.closest('.chat-contact, .conversation-row');
            const traderId = element.getAttribute('data-trader-id');
            if (traderId) {
                await markMessagesAsRead(traderId);
                loadUnreadBadge();
            }
        }
    });
});

// Function to load and display the unread count badge
async function loadUnreadBadge() {
    try {
        const response = await fetch('/api/chat/unread-count');
        const count = await response.json();
        const badge = document.getElementById("unreadBadge");
        
        if (badge) {
            if (count > 0) {
                badge.innerText = count;
                badge.style.display = 'inline-block';
            } else {
                badge.style.display = 'none';
            }
        }
        
        // Update conversation list styling
        updateConversationList();
    } catch (error) {
        console.error('Error fetching unread count:', error);
    }
}

// Function to mark messages as read when a user opens a conversation
async function markMessagesAsRead(traderId) {
    try {
        await fetch(`/api/chat/mark-read/${traderId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });
    } catch (error) {
        console.error('Error marking messages as read:', error);
    }
}

// Function to update conversation list with bold formatting for unread messages
function updateConversationList() {
    // Get all conversation rows
    document.querySelectorAll('.chat-contact, .conversation-row').forEach(row => {
        const unreadCount = parseInt(row.getAttribute('data-unread-count')) || 0;
        if (unreadCount > 0) {
            row.classList.add("font-weight-bold");
            // Show unread count badge next to the conversation
            let badge = row.querySelector('.unread-count-badge');
            if (!badge) {
                badge = document.createElement('span');
                badge.className = 'badge badge-danger ml-2 unread-count-badge';
                const nameElement = row.querySelector('.contact-name, .conversation-name');
                if (nameElement) {
                    nameElement.appendChild(badge);
                }
            }
            badge.innerText = unreadCount > 99 ? '99+' : unreadCount.toString();
        } else {
            row.classList.remove("font-weight-bold");
            const badge = row.querySelector('.unread-count-badge');
            if (badge) {
                badge.remove();
            }
        }
    });
}