/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, Input, OnInit, ViewChild, ElementRef } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { AppLanguageDto, FieldDto } from 'shared';

@Component({
    selector: 'sqx-content-field',
    styleUrls: ['./content-field.component.scss'],
    templateUrl: './content-field.component.html'
})
export class ContentFieldComponent implements OnInit {
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
    public richTextEditorOptions: any;

    @ViewChild('assets')
    public assetLink: ElementRef;

    private buildRichTextEditorOptions() {
        // const self = this;
        return {
            toolbar: 'undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | image assets',
            plugins: 'code,image',
            file_picker_types: 'image',
            convert_urls: false,
            onSetup: (editor: any) => {
                 editor.addButton('assets', {
                    text: '',
                    icon: 'browse',
                    tooltip: 'Insert Assets',
                    onclick: () => {
                        console.log(this.assetLink);
                    }
                 });
            }
        };
    }

    public selectLanguage(language: AppLanguageDto) {
        this.fieldPartition = language.iso2Code;
    }

    public ngOnInit() {
        this.masterLanguageCode = this.languages.find(l => l.isMaster).iso2Code;
        this.richTextEditorOptions = this.buildRichTextEditorOptions();

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

    public selectFieldLanguage(partition: string) {
        return partition === 'iv' ? this.masterLanguageCode : partition;
    }
}

