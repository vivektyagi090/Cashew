import { Component, OnInit, OnDestroy, signal, computed, effect, untracked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

interface StoryScene {
  id: number;
  title: string;
  subtitle: string;
  description: string;
}

@Component({
  selector: 'app-hero-story',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './hero-story.html',
  styleUrl: './hero-story.css'
})
export class HeroStoryComponent implements OnInit, OnDestroy {
  readonly currentSceneIndex = signal(0);
  readonly typedDescription = signal('');
  private intervalId?: any;
  private typingInterval?: any;

  constructor() {
    effect(() => {
      const scene = this.currentScene();
      untracked(() => this.startTyping(scene.description));
    });
  }

  readonly scenes: StoryScene[] = [
    { id: 1, title: 'The Birth of a Champ 🌿', subtitle: 'Sun-Kissed Origins', description: 'Once upon a time, in the lush groves of Maharashtra, a little Cashew Champ was born...' },
    { id: 2, title: 'The Gentle Harvest 🧺', subtitle: 'Hand-Picked with Love', description: 'He was picked with tenderness by caring hands, ensuring only the bravest nuts made the journey.' },
    { id: 3, title: 'The Academy of Excellence 🔬', subtitle: 'Tested for Greatness', description: 'Our Champ went to the "Cashew Academy", where he was tested and graded for his royal crunch.' },
    { id: 4, title: 'The Golden Seal 🎁', subtitle: 'Dressed for Success', description: 'Then, he was dressed in a golden seal of protection, ready for his biggest mission yet.' },
    { id: 5, title: 'The Grand Journey 🚚', subtitle: 'Over Hills & Cities', description: 'Through winding roads and busy cities, the Champ traveled far to find a special home.' },
    { id: 6, title: 'A Joyful Arrival ❤️', subtitle: 'Smiles All Around', description: 'Finally, he arrived! Bringing health and happiness to a lovely family just like yours.' },
    { id: 7, title: 'Invite the Champ Home ✨', subtitle: 'Experience the Miracle', description: 'Now, you can invite the Cashew Champ to your family. Experience the Royal legacy today!' }
  ];

  readonly currentScene = computed(() => this.scenes[this.currentSceneIndex()]);

  ngOnInit() {
    this.startAutoPlay();
  }

  ngOnDestroy() {
    this.stopAutoPlay();
  }

  startAutoPlay() {
    this.stopAutoPlay();
    this.intervalId = setInterval(() => {
      this.nextScene();
    }, 6000); // 6 seconds per scene for cinematic feel
  }

  stopAutoPlay() {
    if (this.intervalId) clearInterval(this.intervalId);
  }

  nextScene() {
    this.currentSceneIndex.update(idx => (idx + 1) % this.scenes.length);
  }

  prevScene() {
    this.currentSceneIndex.update(idx => (idx - 1 + this.scenes.length) % this.scenes.length);
  }

  startTyping(text: string) {
    if (this.typingInterval) clearInterval(this.typingInterval);
    this.typedDescription.set('');
    let i = 0;
    this.typingInterval = setInterval(() => {
      if (i < text.length) {
        this.typedDescription.update(prev => prev + text.charAt(i));
        i++;
      } else {
        clearInterval(this.typingInterval);
      }
    }, 40); // 40ms per character for smooth typing
  }

  goToScene(index: number) {
    this.currentSceneIndex.set(index);
    this.startAutoPlay(); // Reset timer on manual nav
  }
}
