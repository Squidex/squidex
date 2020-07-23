/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { AssetsFieldPropertiesDto, FieldDto } from '@app/shared';

@Component({
    selector: 'sqx-assets-ui',
    styleUrls: ['assets-ui.component.scss'],
    templateUrl: 'assets-ui.component.html'
})
export class AssetsUIComponent implements OnInit {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: AssetsFieldPropertiesDto;

    public ngOnInit() {
        this.fieldForm.setControl('previewMode',
            new FormControl(this.properties.previewMode));

        this.fieldForm.setControl('resolveFirst',
            new FormControl(this.properties.resolveFirst));
    }
}