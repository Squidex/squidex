/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { EMPTY, Observable, shareReplay } from 'rxjs';
import { AppsState, AssetCompletions, AssetScriptsState, AssetsService, EditAssetScriptsForm, ResourceOwner } from '@app/shared';

@Component({
    selector: 'sqx-asset-scripts-page',
    styleUrls: ['./asset-scripts-page.component.scss'],
    templateUrl: './asset-scripts-page.component.html',
})
export class AssetScriptsPageComponent extends ResourceOwner implements OnInit {
    public assetScript = 'annotate';
    public assetCompletions: Observable<AssetCompletions> = EMPTY;

    public editForm = new EditAssetScriptsForm();

    public isEditable = false;

    constructor(
        private readonly appsState: AppsState,
        private readonly assetScriptsState: AssetScriptsState,
        private readonly assetsService: AssetsService,
    ) {
        super();
    }

    public ngOnInit() {
        this.assetCompletions = this.assetsService.getCompletions(this.appsState.appName).pipe(shareReplay(1));

        this.assetScriptsState.scripts
            .subscribe(scripts => {
                this.editForm.load(scripts);
            });
        this.assetScriptsState.canUpdate
            .subscribe(canUpdate => {
                this.isEditable = canUpdate;

                this.editForm.setEnabled(this.isEditable);
            });

        this.assetScriptsState.load();
    }

    public reload() {
        this.assetScriptsState.load(true);
    }

    public selectField(field: string) {
        this.assetScript = field;
    }

    public saveScripts() {
        if (!this.isEditable) {
            return;
        }

        const value = this.editForm.submit();

        if (value) {
            this.assetScriptsState.update(value)
                .subscribe({
                    next: () => {
                        this.editForm.submitCompleted({ noReset: true });
                    },
                    error: error => {
                        this.editForm.submitFailed(error);
                    },
                });
        }
    }
}
