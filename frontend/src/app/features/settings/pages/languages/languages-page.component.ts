/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LanguagesState, LayoutComponent, ListViewComponent, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { LanguageAddFormComponent } from './language-add-form.component';
import { LanguageComponent } from './language.component';

@Component({
    selector: 'sqx-languages-page',
    styleUrls: ['./languages-page.component.scss'],
    templateUrl: './languages-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        LanguageAddFormComponent,
        LanguageComponent,
        LayoutComponent,
        ListViewComponent,
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
export class LanguagesPageComponent {
    constructor(
        protected readonly languagesState: LanguagesState,
    ) {
        this.languagesState.load();
    }

    protected reload() {
        this.languagesState.load(true);
    }
}
