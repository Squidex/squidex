/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { EMPTY, Observable, shareReplay } from 'rxjs';
import { CodeEditorComponent, KeysPipe, LayoutComponent, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { AppsState, AssetScriptsState, AssetsService, EditAssetScriptsForm, ScriptCompletions } from '@app/shared';
import { ScriptNamePipe } from '@app/shared/components/pipes';

@Component({
    selector: 'sqx-asset-scripts-page',
    styleUrls: ['./asset-scripts-page.component.scss'],
    templateUrl: './asset-scripts-page.component.html',
    standalone: true,
    imports: [
        TitleComponent,
        LayoutComponent,
        TooltipDirective,
        ShortcutDirective,
        FormsModule,
        ReactiveFormsModule,
        NgFor,
        NgIf,
        CodeEditorComponent,
        SidebarMenuDirective,
        RouterLink,
        RouterLinkActive,
        TourStepDirective,
        RouterOutlet,
        AsyncPipe,
        KeysPipe,
        TranslatePipe,
        ScriptNamePipe,
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
