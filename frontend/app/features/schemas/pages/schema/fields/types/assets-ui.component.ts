/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { AssetsFieldPropertiesDto, FieldDto } from '@app/shared';

@Component({
    selector: 'sqx-assets-ui',
    styleUrls: ['assets-ui.component.scss'],
    templateUrl: 'assets-ui.component.html'
})
export class AssetsUIComponent implements OnChanges {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: AssetsFieldPropertiesDto;

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['fieldForm']) {
            this.fieldForm.setControl('previewMode',
                new FormControl());

            this.fieldForm.setControl('resolveFirst',
                new FormControl());

            this.fieldForm.setControl('folderId',
                new FormControl());
        }

        this.fieldForm.patchValue(this.properties);
    }
}