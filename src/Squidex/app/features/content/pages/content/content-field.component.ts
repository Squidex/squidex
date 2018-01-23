/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { AppLanguageDto, FieldDto } from 'shared';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
    selector: 'sqx-content-field',
    styleUrls: ['./content-field.component.scss'],
    templateUrl: './content-field.component.html'
})
export class ContentFieldComponent implements OnInit {
    constructor(private readonly router: Router, private readonly route: ActivatedRoute) {
    }
    private masterLanguageCode: string;

    @Input()
    public field: FieldDto;

    @Input()
    public fieldForm: FormGroup;

    @Input()
    public languages: AppLanguageDto[];

    @Input()
    public contentFormSubmitted: boolean;

    public fieldPartitions: string[];
    public fieldPartition: string;

    public selectLanguage(language: AppLanguageDto) {
        this.fieldPartition = language.iso2Code;
    }

    public ngOnInit() {
        this.masterLanguageCode = this.languages.find(l => l.isMaster)!.iso2Code;

        if (this.field.isDisabled) {
            this.fieldForm.disable();
        }

        if (this.field.partitioning === 'language') {
            this.fieldPartitions = this.languages.map(t => t.iso2Code);
            this.fieldPartition = this.fieldPartitions[0];
        } else {
            this.fieldPartitions = ['iv'];
            this.fieldPartition = 'iv';
        }
    }

    public assetPluginClicked() {
        this.router.navigate(['assets'], { relativeTo: this.route });
    }

    public selectFieldLanguage(partition: string) {
        return partition === 'iv' ? this.masterLanguageCode : partition;
    }
}

