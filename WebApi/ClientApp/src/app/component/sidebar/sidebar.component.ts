import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface MenuItem {
    icon: string;
    label: string;
    route: string;
    badge?: number;
}

@Component({
    selector: 'app-sidebar',
    imports: [CommonModule],
    templateUrl: './sidebar.component.html',
    styleUrl: './sidebar.component.css',
})
export class SidebarComponent {
    @Input() isOpen = true;
    @Input() isMobileOpen = false;
    @Input() menuItems: MenuItem[] = [];

    @Output() closeMobile = new EventEmitter<void>();

    onCloseMobile() {
        this.closeMobile.emit();
    }
}
