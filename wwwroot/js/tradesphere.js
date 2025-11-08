// TradeSphere Interactive Enhancements

document.addEventListener('DOMContentLoaded', function() {
    
    // Add loading states to buttons
    const buttons = document.querySelectorAll('.btn');
    buttons.forEach(button => {
        if (button.type === 'submit' || button.tagName.toLowerCase() === 'button') {
            button.addEventListener('click', function() {
                if (!this.classList.contains('btn-outline-danger')) { // Don't add loading to delete buttons
                    this.classList.add('btn-loading');
                    
                    // Remove loading state after 3 seconds as fallback
                    setTimeout(() => {
                        this.classList.remove('btn-loading');
                    }, 3000);
                }
            });
        }
    });
    
    // Animate cards on hover
    const cards = document.querySelectorAll('.card, .dashboard-card');
    cards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-4px)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
        });
    });
    
    // Auto-hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert-success, .alert-info');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.transition = 'opacity 0.5s ease';
            alert.style.opacity = '0';
            setTimeout(() => {
                if (alert.parentNode) {
                    alert.parentNode.removeChild(alert);
                }
            }, 500);
        }, 5000);
    });
    
    // Add confirmation dialogs to delete buttons
    const deleteButtons = document.querySelectorAll('a[href*="/Delete"], button[onclick*="delete"]');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            if (!confirm('Are you sure you want to delete this item? This action cannot be undone.')) {
                e.preventDefault();
                return false;
            }
        });
    });
    
    // Smooth scroll for anchor links
    const anchorLinks = document.querySelectorAll('a[href^="#"]');
    anchorLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
    
    // Add ripple effect to buttons
    function createRipple(event) {
        const button = event.currentTarget;
        const circle = document.createElement('span');
        const diameter = Math.max(button.clientWidth, button.clientHeight);
        const radius = diameter / 2;
        
        circle.style.width = circle.style.height = `${diameter}px`;
        circle.style.left = `${event.clientX - button.offsetLeft - radius}px`;
        circle.style.top = `${event.clientY - button.offsetTop - radius}px`;
        circle.classList.add('ripple');
        
        const ripple = button.getElementsByClassName('ripple')[0];
        if (ripple) {
            ripple.remove();
        }
        
        button.appendChild(circle);
    }
    
    // Add ripple effect CSS dynamically
    const rippleStyle = document.createElement('style');
    rippleStyle.textContent = `
        .btn {
            position: relative;
            overflow: hidden;
        }
        
        .ripple {
            position: absolute;
            border-radius: 50%;
            transform: scale(0);
            animation: ripple 0.6s linear;
            background-color: rgba(255, 255, 255, 0.6);
            pointer-events: none;
        }
        
        @keyframes ripple {
            to {
                transform: scale(4);
                opacity: 0;
            }
        }
    `;
    document.head.appendChild(rippleStyle);
    
    // Add chat styling
    const chatStyle = document.createElement('style');
    chatStyle.textContent = `
        .status-indicator {
            display: inline-block;
            width: 8px;
            height: 8px;
            border-radius: 50%;
            margin-right: 5px;
        }
        .status-online {
            background-color: #28a745;
        }
        .status-offline {
            background-color: #aaa;
        }
        .typing-indicator {
            font-style: italic;
            color: #6c757d;
            margin: 0 0 10px 0;
            font-size: 0.85rem;
        }
        .conversation-row.unread {
            font-weight: bold;
        }
        .unread-count-badge {
            background-color: #dc3545;
            color: white;
            font-size: 0.75rem;
            padding: 2px 6px;
            border-radius: 10px;
            margin-left: 8px;
        }
        .last-message-time {
            font-size: 0.75rem;
            color: #6c757d;
        }
    `;
    document.head.appendChild(chatStyle);
    
    // Apply ripple effect to buttons
    buttons.forEach(button => {
        button.addEventListener('click', createRipple);
    });
    
    // Add search functionality if search input exists
    const searchInput = document.querySelector('#searchInput, .search-input, input[placeholder*="search" i]');
    if (searchInput) {
        searchInput.addEventListener('input', function() {
            const searchTerm = this.value.toLowerCase();
            const cards = document.querySelectorAll('.card');
            
            cards.forEach(card => {
                const text = card.textContent.toLowerCase();
                const shouldShow = text.includes(searchTerm);
                card.style.display = shouldShow ? 'block' : 'none';
                
                if (card.closest('.col-lg-4, .col-md-6, .col-sm-12')) {
                    card.closest('.col-lg-4, .col-md-6, .col-sm-12').style.display = shouldShow ? 'block' : 'none';
                }
            });
        });
    }
    
    // Add theme toggle functionality
    function toggleTheme() {
        document.body.classList.toggle('dark-theme');
        localStorage.setItem('theme', document.body.classList.contains('dark-theme') ? 'dark' : 'light');
    }
    
    // Load saved theme
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme === 'dark') {
        document.body.classList.add('dark-theme');
    }
    
    // Add scroll to top button
    const scrollToTopBtn = document.createElement('button');
    scrollToTopBtn.innerHTML = '<i class="fas fa-arrow-up"></i>';
    scrollToTopBtn.className = 'btn btn-primary position-fixed';
    scrollToTopBtn.id = 'scrollToTopBtn';
    scrollToTopBtn.style.cssText = `
        bottom: 20px;
        right: 20px;
        z-index: 1050;
        border-radius: 50%;
        width: 50px;
        height: 50px;
        display: none;
        box-shadow: 0 2px 10px rgba(0,0,0,0.3);
    `;
    
    document.body.appendChild(scrollToTopBtn);
    
    // Show/hide scroll to top button
    window.addEventListener('scroll', function() {
        if (window.pageYOffset > 300) {
            scrollToTopBtn.style.display = 'block';
        } else {
            scrollToTopBtn.style.display = 'none';
        }
    });
    
    // Scroll to top functionality
    scrollToTopBtn.addEventListener('click', function() {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    });
    
    // Console welcome message
    console.log('%c🚀 Welcome to TradeSphere!', 'color: #2563eb; font-size: 16px; font-weight: bold;');
    console.log('%cBuilt with modern web technologies for the best trading experience.', 'color: #64748b; font-size: 12px;');
});

// Chat functionality with unread count
if (typeof signalR !== 'undefined') {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/chatHub')
        .build();
    
    // Make connection available globally
    window.connection = connection;

    // Set up SignalR connection
    connection.start().catch(err => console.error(err.toString()));

    // Listen for new messages
    connection.on("ReceiveMessage", (message) => {
        const currentTraderId = getCurrentTraderId();
        if (message.SenderTraderId && message.SenderTraderId !== currentTraderId) {
            moveConversationToTop(message.SenderTraderId);
            incrementUnreadCount(message.SenderTraderId);
        }
        loadUnreadBadge();
    });
    
    connection.on("UpdateChatBoard", refreshChatBoard);
    connection.on("UserStatusChanged", refreshChatBoard);
    
    connection.on("UserTyping", (senderTraderId) => {
        const typingElement = document.getElementById("typingIndicator");
        if (typingElement) {
            typingElement.innerText = "Typing...";
            clearTimeout(window.typingTimeout);
            window.typingTimeout = setTimeout(() => {
                typingElement.innerText = "";
            }, 3000);
        }
    });
    
    // Load unread count on page load
    loadUnreadBadge();
    
    // Mark messages as read when chat opens
    document.addEventListener('click', async event => {
        const element = event.target.closest('.chat-contact, .conversation-row');
        if (element) {
            const traderId = element.getAttribute('data-trader-id');
            if (traderId) {
                await markMessagesAsRead(traderId);
                loadUnreadBadge();
            }
        }
    });
    
    // Typing indicator trigger
    const messageInput = document.getElementById('messageInput');
    if (messageInput) {
        let typingTimer;
        messageInput.addEventListener('input', () => {
            const targetTraderId = messageInput.getAttribute('data-target-trader-id');
            if (targetTraderId && window.connection) {
                clearTimeout(typingTimer);
                typingTimer = setTimeout(() => {
                    window.connection.invoke("Typing", parseInt(targetTraderId));
                }, 300);
            }
        });
    }
                    }
                }, 300);
            }
        });
    }
}

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

// Function to move conversation to top of the list
function moveConversationToTop(senderTraderId) {
    const conversationContainer = document.querySelector('.chat-contacts-list, .conversation-list');
    if (!conversationContainer) return;
    
    const senderElement = conversationContainer.querySelector(`[data-trader-id="${senderTraderId}"]`);
    if (senderElement && senderElement.parentNode === conversationContainer) {
        // Move to top
        conversationContainer.insertBefore(senderElement, conversationContainer.firstChild);
        // Update timestamp to "just now"
        const timeElement = senderElement.querySelector('.last-message-time');
        if (timeElement) {
            timeElement.textContent = 'just now';
        }
    }
}

// Function to increment unread count for a conversation
function incrementUnreadCount(senderTraderId) {
    const senderElement = document.querySelector(`[data-trader-id="${senderTraderId}"]`);
    if (!senderElement) return;
    
    const currentCount = parseInt(senderElement.getAttribute('data-unread-count')) || 0;
    senderElement.setAttribute('data-unread-count', currentCount + 1);
    
    // Update DOM display
    updateConversationBadge(senderElement, currentCount + 1);
    // Make it bold
    senderElement.classList.add("font-weight-bold");
}

// Helper to update badge UI
function updateConversationBadge(element, count) {
    let badge = element.querySelector('.unread-count-badge');
    if (!badge) {
        badge = document.createElement('span');
        badge.className = 'badge badge-danger ml-2 unread-count-badge';
        const nameElement = element.querySelector('.contact-name, .conversation-name');
        if (nameElement) {
            nameElement.appendChild(badge);
        }
    }
    badge.innerText = count > 99 ? '99+' : count.toString();
}

// Get current trader ID from page or localStorage
function getCurrentTraderId() {
    // Try to get current trader ID from several possible sources
    return document.querySelector('meta[name="trader-id"]')?.content ||
           localStorage.getItem('traderId') ||
           document.querySelector('#currentTraderId')?.value ||
           '';
}

// Refresh Chat Board function
async function refreshChatBoard() {
    try {
        const response = await fetch('/api/chat/chat-board-partial');
        const html = await response.text();
        const container = document.getElementById("chatBoardContainer");
        if (container) {
            container.innerHTML = html;
        }
    } catch (error) {
        console.error('Error refreshing chat board:', error);
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

// Utility functions
window.TradeSphere = {
    showNotification: function(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} position-fixed`;
        notification.style.cssText = `
            top: 20px;
            right: 20px;
            z-index: 1060;
            min-width: 300px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        `;
        notification.innerHTML = `
            <div class="d-flex justify-content-between align-items-center">
                <span>${message}</span>
                <button type="button" class="btn-close btn-close-white" onclick="this.parentElement.parentElement.remove()"></button>
            </div>
        `;
        
        document.body.appendChild(notification);
        
        // Auto remove after 5 seconds
        setTimeout(() => {
            if (notification.parentNode) {
                notification.remove();
            }
        }, 5000);
    },
    
    formatCurrency: function(amount) {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD'
        }).format(amount);
    },
    
    formatDate: function(date) {
        return new Intl.DateTimeFormat('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        }).format(new Date(date));
    }
};