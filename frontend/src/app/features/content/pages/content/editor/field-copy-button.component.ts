/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { AppLanguageDto, FieldForm, ModalModel } from '@app/shared';

@Component({
    selector: 'sqx-field-copy-button[formModel][languages]',
    styleUrls: ['./field-copy-button.component.scss'],
    templateUrl: './field-copy-button.component.html',
})
export class FieldCopyButtonComponent implements OnChanges {
    @Input()
    public formModel!: FieldForm;

    @Input()
    public languages!: ReadonlyArray<AppLanguageDto>;

    public languageCodes: ReadonlyArray<string> = [];

    public copySource = '';
    public copyTargets?: ReadonlyArray<string>;

    public dropdown = new ModalModel();

    public get isLocalized() {
        return this.formModel.field.isLocalizable && this.languages.length > 1;
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['languages']) {
            this.setCopySource(this.languages[0]?.iso2Code);
        }
    }

    public setCopySource(language: string) {
        this.copySource = language;
        this.copyTargets = [];

        this.languageCodes = this.languages.map(x => x.iso2Code).removed(language);
    }

    public copy() {
        if (this.copySource && this.copyTargets && this.copyTargets?.length > 0) {
            const source = this.formModel.get(this.copySource).form.value;

            for (const target of this.copyTargets) {
                if (target !== this.copySource) {
                    this.formModel.get(target)?.setValue(source);
                }
            }
        }
    }
}
