/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { AppLanguageDto, RootFieldDto } from '@app/shared';

@Component({
    selector: 'sqx-field-languages[field][language][languages]',
    styleUrls: ['./field-languages.component.scss'],
    templateUrl: './field-languages.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FieldLanguagesComponent {
    @Output()
    public showAllControlsChange = new EventEmitter<boolean>();

    @Input()
    public showAllControls?: boolean | null;

    @Output()
    public languageChange = new EventEmitter<AppLanguageDto>();

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: ReadonlyArray<AppLanguageDto>;

    @Input()
    public field: RootFieldDto;

    public toggleShowAllControls() {
        this.showAllControlsChange.emit(!this.showAllControls);
    }
}
