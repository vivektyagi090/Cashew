import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GoogleGenerativeAI } from "@google/generative-ai";

interface ChatMessage {
    text: string;
    isUser: boolean;
    timestamp: Date;
}

@Component({
    selector: 'app-chatbot',
    imports: [CommonModule, FormsModule],
    templateUrl: './chatbot.component.html',
    styleUrl: './chatbot.component.css',
})
export class ChatbotComponent {
    // Inside your class:
    // private genAI = new GoogleGenerativeAI("AIzaSyDYS-9fEcwKbKFW0MMR1NsQ0jr4Z6EIEUY");
    // 1. Initialize Gemini with your API Key

    // Replace this with the key that worked in Postman
    private genAI = new GoogleGenerativeAI("AIzaSyDYS-9fEcwKbKFW0MMR1NsQ0jr4Z6EIEUY");
    private model = this.genAI.getGenerativeModel({ model: "gemini-1.5-flash" });
    isOpen = signal(false);
    messages = signal<ChatMessage[]>([
        {
            text: 'Hello! I\'m Vivek Assistant. How can I help you today?',
            isUser: false,
            timestamp: new Date()
        }
    ]);
    userInput = '';
    isTyping = signal(false); // Added a typing indicator for better UX
    toggleChat() {
        this.isOpen.update(val => !val);
    }
    // 2. Change to async to handle the API response
    async sendMessage() {
        const textToSend = this.userInput.trim();
        if (!textToSend) return;

        // Add user message to UI
        this.messages.update(msgs => [...msgs, {
            text: textToSend,
            isUser: true,
            timestamp: new Date()
        }]);

        this.userInput = '';
        this.isTyping.set(true); // Show "Bot is typing..."

        try {
            // 3. Call the Gemini API
            const result = await this.model.generateContent(textToSend);
            const response = await result.response;
            const botResponse = response.text();

            // Add AI response to UI
            this.messages.update(msgs => [...msgs, {
                text: botResponse,
                isUser: false,
                timestamp: new Date()
            }]);
        } catch (error) {
            console.error("Gemini Error:", error);
            this.messages.update(msgs => [...msgs, {
                text: "Sorry, I'm having trouble connecting right now. Please check your API key.",
                isUser: false,
                timestamp: new Date()
            }]);
        } finally {
            this.isTyping.set(false); // Hide "Bot is typing..."
        }
    }
    // sendMessage() {
    //     if (!this.userInput.trim()) return;

    //     // Add user message
    //     this.messages.update(msgs => [...msgs, {
    //         text: this.userInput,
    //         isUser: true,
    //         timestamp: new Date()
    //     }]);

    //     const userMessage = this.userInput.toLowerCase();
    //     this.userInput = '';

    //     // Simulate bot response
    //     setTimeout(() => {
    //         let botResponse = '';

    //         if (userMessage.includes('hello') || userMessage.includes('hi')) {
    //             botResponse = 'Hello! How can I assist you with RKCMS today?';
    //         } else if (userMessage.includes('help')) {
    //             botResponse = 'I can help you with:\n• Creating content\n• Managing users\n• Viewing analytics\n• System settings\n\nWhat would you like to know more about?';
    //         } else if (userMessage.includes('user')) {
    //             botResponse = 'To add a new user, click the "Create New" button in the dashboard. You can also manage existing users from the Users section in the sidebar.';
    //         } else if (userMessage.includes('content')) {
    //             botResponse = 'You can create and manage content from the Content section. Click on "Content" in the sidebar to get started.';
    //         } else {
    //             botResponse = 'I\'m a demo chatbot. I can help you navigate RKCMS. Try asking about users, content, or help!';
    //         }

    //         this.messages.update(msgs => [...msgs, {
    //             text: botResponse,
    //             isUser: false,
    //             timestamp: new Date()
    //         }]);
    //     }, 500);
    // }

    // handleKeyPress(event: KeyboardEvent) {
    //     if (event.key === 'Enter' && !event.shiftKey) {
    //         event.preventDefault();
    //         this.sendMessage();
    //     }
    // }
    handleKeyPress(event: KeyboardEvent) {
        if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault();
            this.sendMessage();
        }
    }
}
