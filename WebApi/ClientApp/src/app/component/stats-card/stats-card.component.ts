import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-stats-card',
    imports: [CommonModule],
    templateUrl: './stats-card.component.html',
    styleUrl: './stats-card.component.css',
})
export class StatsCardComponent {
    @Input() icon: string = 'default';
    @Input() label: string = '';
    @Input() value: string = '0';
    @Input() change: string = '';
    @Input() type: 'primary' | 'success' | 'warning' | 'info' = 'primary';
    @Input() isPositive: boolean = true;
}
