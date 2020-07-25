import { Pipe, PipeTransform } from '@angular/core';
import { LocalizerService } from '@app/framework/services/localizer.service';

@Pipe({
    name: 'sqxTranslate',
    pure: true
  })
  export class SqxTranslatePipe implements PipeTransform {

    constructor(private readonly localizer: LocalizerService) {
    }

    public transform(value: string, args?: readonly any[]): string {
        return this.localizer.get(value, args);
    }
  }