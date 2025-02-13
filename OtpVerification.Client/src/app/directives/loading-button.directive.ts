import { Directive, ElementRef, Input } from '@angular/core';

@Directive({
  selector: '[appLoadingButton]',
  standalone: true
})
export class LoadingButtonDirective {
  @Input() appLoadingButton: boolean = false;

  private originalContent: string | null = null;

  private get nativeElement(): HTMLButtonElement {
    return this.el.nativeElement;
  }
  
  constructor(private el: ElementRef) {
    if (this.nativeElement.tagName.toLowerCase() !== 'button') {
      throw new Error('appLoadingButton directive can only be used on <button> elements.');
    }
  }

  ngOnChanges(): void {
    if (this.originalContent === null) {
      this.originalContent = this.nativeElement.innerHTML;
    }

    if (this.appLoadingButton) {
      this.showSpinner();
    } else {
      this.hideSpinner();
    }
  }

  private showSpinner(): void {
    this.nativeElement.innerHTML = '<span class="spinner-grow spinner-grow-sm"></span>';
    this.nativeElement.disabled = true;
  }

  private hideSpinner(): void {
    this.nativeElement.innerHTML = this.originalContent!;
    this.nativeElement.disabled = false;
  }

}
