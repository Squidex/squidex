/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';
import { ApiUrlConfig, AuthService, fadeAnimation, FieldDto, ModalModel, StatefulComponent, UIOptions, UIState } from '@app/shared';


@Pipe({
    name: 'sqxLocalized'
})

export class LocalizedPipe implements PipeTransform {
    
    constructor(public readonly uiOptions: UIOptions) { }

    public language = this.uiOptions.get('more.culture');
    public field: FieldDto;
    
    transform(value: FieldDto, args?: any) : string {
        if (!value.properties.localizedLabel) {
            return "";
          }
          
          return value.properties.localizedLabel[language] || value.properties.localizedLabel['en'] || Object.values(value.properties.localizedLabel)[0];
    }
}