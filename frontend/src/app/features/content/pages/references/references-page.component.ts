/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, linkedSignal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { distinctUntilChanged, map } from 'rxjs/operators';
import { AppLanguageDto, ComponentContentsState, ContentsColumnsPipe, LanguageSelectorComponent, LanguagesState, LayoutComponent, ListViewComponent, PagerComponent, QuerySynchronizer, Router2State, ShortcutDirective, TitleComponent, TooltipDirective, TranslatePipe } from '@app/shared';
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
        AsyncPipe,
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
    private readonly languageMaster = toSignal(this.languagesState.isoMasterLanguage, { initialValue: null! });

    public readonly language = linkedSignal<AppLanguageDto>(() => {
        return this.languageMaster()!;
    });

    public readonly languages = toSignal(this.languagesState.isoLanguages, { initialValue: [] });

    constructor(
        public readonly contentsRoute: Router2State,
        public readonly contentsState: ComponentContentsState,
        public readonly languagesState: LanguagesState,
        private readonly route: ActivatedRoute,
    ) {
        effect(onCleanup => {
            const subscription =
                getReferenceId(this.route)
                    .subscribe(referenceId => {
                        const initial =
                            this.contentsRoute.mapTo(this.contentsState)
                                .withPaging('contents', 10)
                                .withSynchronizer(QuerySynchronizer.INSTANCE)
                                .getInitial();

                        this.contentsState.schema = { name: null! };
                        this.contentsState.loadReferences(referenceId, initial);
                        this.contentsRoute.listen();
                    });

            onCleanup(() => subscription.unsubscribe());
        });
    }

    public reload() {
        this.contentsState.load(true);
    }

    public changeLanguage(language: AppLanguageDto) {
        this.language.set(language);
    }
}

function getReferenceId(route: ActivatedRoute) {
    return route.params.pipe(map(x => x['referenceId'] as string), distinctUntilChanged());
}
