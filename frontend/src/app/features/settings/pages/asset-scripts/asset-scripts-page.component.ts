/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { EMPTY, Observable, shareReplay } from 'rxjs';
import { AppsState, AssetScriptsState, AssetsService, CodeEditorComponent, EditAssetScriptsForm, KeysPipe, LayoutComponent, ScriptCompletions, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { ScriptNamePipe } from '@app/shared/components/pipes';

@Component({
    standalone: true,
    selector: 'sqx-asset-scripts-page',
    styleUrls: ['./asset-scripts-page.component.scss'],
    templateUrl: './asset-scripts-page.component.html',
    imports: [
        AsyncPipe,
        CodeEditorComponent,
        FormsModule,
        KeysPipe,
        LayoutComponent,
        ReactiveFormsModule,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        ScriptNamePipe,
        ShortcutDirective,
        SidebarMenuDirective,
        TitleComponent,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class AssetScriptsPageComponent implements OnInit {
    public assetScript = 'query';
    public assetCompletions: Observable<ScriptCompletions> = EMPTY;

    public editForm = new EditAssetScriptsForm();

    public isEditable = false;

    constructor(
        private readonly appsState: AppsState,
        private readonly assetScriptsState: AssetScriptsState,
        private readonly assetsService: AssetsService,
    ) {
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
