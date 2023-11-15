/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LanguagesState, LayoutComponent, ListViewComponent, ShortcutDirective, SidebarMenuDirective, SnapshotLanguage, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { LanguageAddFormComponent } from './language-add-form.component';
import { LanguageComponent } from './language.component';

@Component({
    selector: 'sqx-languages-page',
    styleUrls: ['./languages-page.component.scss'],
    templateUrl: './languages-page.component.html',
    standalone: true,
    imports: [
        TitleComponent,
        LayoutComponent,
        TooltipDirective,
        ShortcutDirective,
        ListViewComponent,
        NgIf,
        LanguageAddFormComponent,
        NgFor,
        LanguageComponent,
        SidebarMenuDirective,
        RouterLink,
        RouterLinkActive,
        TourStepDirective,
        RouterOutlet,
        AsyncPipe,
        TranslatePipe,
    ],
})
export class LanguagesPageComponent implements OnInit {
    constructor(
        public readonly languagesState: LanguagesState,
    ) {
    }

    public ngOnInit() {
        this.languagesState.load();
    }

    public reload() {
        this.languagesState.load(true);
    }

    public trackByLanguage(_index: number, language: SnapshotLanguage) {
        return language.language.iso2Code;
    }
}
