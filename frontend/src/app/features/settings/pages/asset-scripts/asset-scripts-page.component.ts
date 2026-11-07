/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Observable, shareReplay } from 'rxjs';
import { AppsState, AssetScriptsState, AssetsService, CodeEditorComponent, EditAssetScriptsForm, KeysPipe, LayoutComponent, ScriptCompletions, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { ScriptNamePipe } from '@app/shared/components/pipes';

@Component({
    selector: 'sqx-asset-scripts-page',
    styleUrls: ['./asset-scripts-page.component.scss'],
    templateUrl: './asset-scripts-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
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
export class AssetScriptsPageComponent {
    public readonly assetScript = signal('query');
    public readonly assetCompletions: Observable<ScriptCompletions>;

    public readonly editForm = new EditAssetScriptsForm();

    public readonly isEditable = toSignal(this.assetScriptsState.canUpdate, { initialValue: false });

    constructor(appsState: AppsState, assetsService: AssetsService,
        private readonly assetScriptsState: AssetScriptsState,
    ) {
        this.assetCompletions = assetsService.getCompletions(appsState.appName).pipe(shareReplay(1));

        effect(onCleanup => {
            const subscription = this.assetScriptsState.scripts
                .subscribe(scripts => {
                    this.editForm.load(scripts);
                });

            onCleanup(() => subscription.unsubscribe());
        });

        effect(() => {
            this.editForm.setEnabled(this.isEditable());
        });

        this.assetScriptsState.load();
    }

    public reload() {
        this.assetScriptsState.load(true);
    }

    public selectField(field: string) {
        this.assetScript.set(field);
    }

    public saveScripts() {
        if (!this.isEditable()) {
            return;
        }

        const value = this.editForm.submit();
        if (!value) {
            return;
        }

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
