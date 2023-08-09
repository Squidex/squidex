/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { AppLanguageDto, FieldForm } from '@app/shared';

@Component({
    selector: 'sqx-field-languages',
    styleUrls: ['./field-languages.component.scss'],
    templateUrl: './field-languages.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FieldLanguagesComponent {
    @Output()
    public showAllControlsChange = new EventEmitter<boolean>();

    @Input({ transform: booleanAttribute })
    public showAllControls?: boolean | null;

    @Output()
    public languageChange = new EventEmitter<AppLanguageDto>();

    @Input({ required: true })
    public language!: AppLanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<AppLanguageDto>;

    @Input({ required: true })
    public formModel!: FieldForm;

    public toggleShowAllControls() {
        this.showAllControlsChange.emit(!this.showAllControls);
    }
}
