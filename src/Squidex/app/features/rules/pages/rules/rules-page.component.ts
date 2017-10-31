/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';

import {
    AppComponentBase,
    AppsStoreService,
    AuthService,
    DialogService,
    fadeAnimation,
    ImmutableArray,
    ModalView,
    RuleDto,
    RulesService,
    SchemaDto,
    SchemasService
} from 'shared';

@Component({
    selector: 'sqx-rules-page',
    styleUrls: ['./rules-page.component.scss'],
    templateUrl: './rules-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class RulesPageComponent extends AppComponentBase implements OnInit {
    public addRuleDialog = new ModalView(true, false);

    public rules: ImmutableArray<RuleDto>;
    public schemas: SchemaDto[];

    constructor(apps: AppsStoreService, dialogs: DialogService, authService: AuthService,
        private readonly schemasService: SchemasService,
        private readonly rulesService: RulesService
    ) {
        super(dialogs, apps, authService);
    }

    public ngOnInit() {
        this.load();
    }

    public load(showInfo = false) {
        this.appNameOnce()
            .switchMap(app =>
                this.schemasService.getSchemas(app)
                    .combineLatest(this.rulesService.getRules(app),
                        (s, w) => { return { rules: w, schemas: s }; }))
            .subscribe(dtos => {
                this.schemas = dtos.schemas;
                this.rules = ImmutableArray.of(dtos.rules);

                if (showInfo) {
                    this.notifyInfo('Rules reloaded.');
                }
            }, error => {
                this.notifyError(error);
            });
    }
}
