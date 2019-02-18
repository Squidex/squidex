/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import { AppLanguageDto, RootFieldDto } from '@app/shared';

@Component({
    selector: 'sqx-field-languages',
    template: `
        <ng-container *ngIf="field.isLocalizable && languages.length > 1">
            <button *ngIf="!field.properties.isComplexUI" type="button" class="btn btn-text-secondary btn-sm mr-1" (click)="showAllControlsChange.emit(!showAllControls)">
                {{showAllControls ? 'Single Language' : 'All Languages'}}
            </button>

            <ng-container *ngIf="field.properties.isComplexUI || !showAllControls">
                <sqx-language-selector size="sm" #buttonLanguages
                    [selectedLanguage]="language"
                    (selectedLanguageChange)="languageChange.emit($event)"
                    [languages]="languages">
                </sqx-language-selector>

                <sqx-onboarding-tooltip helpId="languages" [for]="buttonLanguages" position="top-right" after="120000">
                    Please remember to check all languages when you see validation errors.
                </sqx-onboarding-tooltip>
            </ng-container>
        </ng-container>`,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FieldLanguagesComponent {
    @Input()
    public field: RootFieldDto;

    @Input()
    public showAllControls: boolean;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: AppLanguageDto[];

    @Output()
    public languageChange = new EventEmitter<AppLanguageDto>();

    @Output()
    public showAllControlsChange = new EventEmitter<AppLanguageDto>();
}