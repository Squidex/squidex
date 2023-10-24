/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { distinctUntilChanged, map } from 'rxjs/operators';
import { AppLanguageDto, ComponentContentsState, ContentDto, LanguagesState, QuerySynchronizer, Router2State, Subscriptions } from '@app/shared';

@Component({
    selector: 'sqx-references-page',
    styleUrls: ['./references-page.component.scss'],
    templateUrl: './references-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        Router2State, ComponentContentsState,
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

    public trackByContent(_index: number, content: ContentDto) {
        return content.id;
    }
}

function getReferenceId(route: ActivatedRoute) {
    return route.params.pipe(map(x => x['referenceId'] as string), distinctUntilChanged());
}