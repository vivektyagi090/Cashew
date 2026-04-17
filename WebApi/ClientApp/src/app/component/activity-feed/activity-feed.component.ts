import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface Activity {
    type: 'create' | 'edit' | 'user' | 'delete';
    text: string;
    time: string;
}

@Component({
    selector: 'app-activity-feed',
    imports: [CommonModule],
    templateUrl: './activity-feed.component.html',
    styleUrl: './activity-feed.component.css',
})
export class ActivityFeedComponent {
    @Input() activities: Activity[] = [];
}
