/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { distinctUntilChanged, map } from 'rxjs/operators';
import { AppLanguageDto, ComponentContentsState, ContentsColumnsPipe, LanguageSelectorComponent, LanguagesState, LayoutComponent, ListViewComponent, PagerComponent, QuerySynchronizer, Router2State, ShortcutDirective, Subscriptions, TitleComponent, TooltipDirective, TranslatePipe } from '@app/shared';
import { ReferenceItemComponent } from '../../shared/references/reference-item.component';

@Component({
    standalone: true,
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
export class ReferencesPageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public language!: AppLanguageDto;
    public languages!: ReadonlyArray<AppLanguageDto>;

    constructor(
        public readonly contentsRoute: Router2State,
        public readonly contentsState: ComponentContentsState,
        public readonly languagesState: LanguagesState,
        private readonly route: ActivatedRoute,
    ) {
    }

    public ngOnInit() {
        this.subscriptions.add(
            this.languagesState.isoMasterLanguage
                .subscribe(language => {
                    this.language = language;
                }));

        this.subscriptions.add(
            this.languagesState.isoLanguages
                .subscribe(languages => {
                    this.languages = languages;
                }));

        this.subscriptions.add(
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
                }));
    }

    public reload() {
        this.contentsState.load(true);
    }

    public changeLanguage(language: AppLanguageDto) {
        this.language = language;
    }
}

function getReferenceId(route: ActivatedRoute) {
    return route.params.pipe(map(x => x['referenceId'] as string), distinctUntilChanged());
}
