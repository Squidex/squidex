/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, effect, inject, linkedSignal, untracked } from '@angular/core';
import { AppLanguageDto, ComponentContentsState, ContentsColumnsPipe, LanguageSelectorComponent, LanguagesState, LayoutComponent, ListViewComponent, PagerComponent, QuerySynchronizer, routeParam, Router2State, ShortcutDirective, TitleComponent, TooltipDirective, TranslatePipe } from '@app/shared';
import { ReferenceItemComponent } from '../../shared/references/reference-item.component';

@Component({
    selector: 'sqx-references-page',
    styleUrls: ['./references-page.component.scss'],
    templateUrl: './references-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        ComponentContentsState,
        Router2State,
    ],
    imports: [
        ContentsColumnsPipe,
        LanguageSelectorComponent,
        LayoutComponent,
        ListViewComponent,
        PagerComponent,
        ReferenceItemComponent,
        ShortcutDirective,
        TitleComponent,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class ReferencesPageComponent {
    private readonly contentsRoute = inject(Router2State);
    private readonly languagesState = inject(LanguagesState);
    private readonly referenceId = routeParam('referenceId');

    protected readonly contentsState = inject(ComponentContentsState);

    protected readonly language = linkedSignal<AppLanguageDto>(() => {
        return this.languagesState.isoMasterLanguage();
    });

    protected readonly languages = computed(() => {
        return this.languagesState.isoLanguages();
    });

    constructor() {
        effect(() => {
            const referenceId = this.referenceId();
            if (!referenceId) {
                return;
            }

            untracked(() => {
                const initial =
                    this.contentsRoute.mapTo(this.contentsState)
                        .withPaging('contents', 10)
                        .withSynchronizer(QuerySynchronizer.INSTANCE)
                        .getInitial();

                this.contentsState.schema = { name: null! };
                this.contentsState.loadReferences(referenceId, initial);
                this.contentsRoute.listen();
            });
        });
    }

    protected reload() {
        this.contentsState.load(true);
    }

    protected changeLanguage(language: AppLanguageDto) {
        this.language.set(language);
    }
}
