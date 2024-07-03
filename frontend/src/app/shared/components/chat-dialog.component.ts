/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { HTTP, MathHelper, ModalDialogComponent, ResizedDirective, TooltipDirective, TranslatePipe } from '@app/framework';
import { AppsState, AuthService, ChatEventDto, StatefulComponent, TranslationsService } from '@app/shared/internal';
import { ChatItemComponent } from './chat-item.component';

interface State {
    // The questions.
    chatQuestion: string;

    // Indicates if an item is running.
    isRunning: boolean;

    // The answers.
    chatItems: ReadonlyArray<{ content: string | Observable<ChatEventDto>; type: 'User' | 'Bot' | 'System' }>;
}

@Component({
    standalone: true,
    selector: 'sqx-chat-dialog',
    styleUrls: ['./chat-dialog.component.scss'],
    templateUrl: './chat-dialog.component.html',
    imports: [
        ChatItemComponent,
        FormsModule,
        ModalDialogComponent,
        ResizedDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class ChatDialogComponent extends StatefulComponent<State> {
    private readonly conversationId = MathHelper.guid();

    @Output()
    public contentSelect = new EventEmitter<string | HTTP.UploadFile | undefined | null>();

    @Input()
    public configuration?: string;

    @Input()
    public folderId?: string;

    @Input()
    public copyMode?: 'Text' | 'Image';

    @ViewChild('input', { static: false })
    public input!: ElementRef<HTMLInputElement>;

    public user = this.authService.user!;

    constructor(
        private readonly appsState: AppsState,
        private readonly authService: AuthService,
        private readonly translator: TranslationsService,
    ) {
        super({
            chatItems: [],
            chatQuestion: '',
            isRunning: false,
        });
    }

    public ngOnInit() {
        const { configuration, conversationId } = this;
        const stream = this.translator.ask(this.appsState.appName, { conversationId, configuration });

        this.next(s => ({
            ...s,
            chatQuestion: '',
            chatItems: [...s.chatItems, { content: stream, type: 'Bot' }],
            isRunning: true,
        }));
    }

    public setQuestion(chatQuestion: string) {
        this.next({ chatQuestion });
    }

    public setCompleted() {
        this.next({ isRunning: false });
    }

    public ask() {
        const prompt = this.snapshot.chatQuestion;

        if (!prompt || prompt.length === 0) {
            return;
        }

        const { configuration, conversationId } = this;
        const stream = this.translator.ask(this.appsState.appName, { prompt, conversationId, configuration });

        this.next(s => ({
            ...s,
            chatQuestion: '',
            chatItems: [
                ...s.chatItems,
                { content: prompt, type: 'User' },
                { content: stream, type: 'Bot' },
            ],
            isRunning: true,
        }));
    }
}
