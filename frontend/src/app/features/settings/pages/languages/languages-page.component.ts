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
    standalone: true,
    selector: 'sqx-languages-page',
    styleUrls: ['./languages-page.component.scss'],
    templateUrl: './languages-page.component.html',
    imports: [
        AsyncPipe,
        LanguageAddFormComponent,
        LanguageComponent,
        LayoutComponent,
        ListViewComponent,
        NgFor,
        NgIf,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        ShortcutDirective,
        SidebarMenuDirective,
        TitleComponent,
        TooltipDirective,
        TourStepDirective,
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
