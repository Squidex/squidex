/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { AppLanguageDto, FieldDto } from 'shared';

@Component({
    selector: 'sqx-content-field',
    styleUrls: ['./content-field.component.scss'],
    templateUrl: './content-field.component.html'
})
export class ContentFieldComponent implements OnInit {
    @Input()
    public field: FieldDto;

    @Input()
    public fieldForm: FormGroup;

    @Input()
    public languages: AppLanguageDto[];

    @Input()
    public contentFormSubmitted: boolean;

    public selectedFormControl: string;
    public selectedLanguage: AppLanguageDto;

    constructor(
        private readonly router: Router,
        private readonly route: ActivatedRoute
    ) {
    }

    public ngOnInit() {
        if (this.field.isDisabled) {
            this.fieldForm.disable();
        }

        const masterLanguage = this.languages.find(l => l.isMaster)!;

        if (this.field.isLocalizable) {
            this.selectedFormControl = masterLanguage.iso2Code;
        } else {
            this.selectedFormControl = 'iv';
        }

        this.selectedLanguage = masterLanguage;
    }

    public selectLanguage(language: AppLanguageDto) {
        this.selectedFormControl = language.iso2Code;
        this.selectedLanguage = language;
    }

    public assetPluginClicked() {
        this.router.navigate(['assets'], { relativeTo: this.route });
    }
}

