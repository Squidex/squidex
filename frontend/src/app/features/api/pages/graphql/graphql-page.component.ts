/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { afterNextRender, Component, DestroyRef, effect, ElementRef, inject, signal, untracked, viewChild } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { form, FormField } from '@angular/forms/signals';
import { createGraphiQLFetcher } from '@graphiql/toolkit';
import { GraphiQL } from 'graphiql';
import * as React from 'react';
import * as ReactDOM from 'react-dom/client';
import { map, of } from 'rxjs';
import { ApiUrlConfig, AppsState, AuthService, ClientDto, ClientsService, ClientsState, DialogModel, FormHintComponent, FormRowComponent, LayoutComponent, MessageBus, ModalDialogComponent, ModalDirective, QueryExecuted, SignalSelectDirective, ThemeService, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe, Types } from '@app/shared';

@Component({
    selector: 'sqx-graphql-page',
    styleUrls: ['./graphql-page.component.scss'],
    templateUrl: './graphql-page.component.html',
    imports: [
        FormField,
        FormHintComponent,
        FormsModule,
        FormRowComponent,
        LayoutComponent,
        ModalDialogComponent,
        ModalDirective,
        SignalSelectDirective,
        TitleComponent,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class GraphQLPageComponent {
    private readonly apiUrl = inject(ApiUrlConfig);
    private readonly appsState = inject(AppsState);
    private readonly authService = inject(AuthService);
    private readonly clientsService = inject(ClientsService);
    private readonly destroyRef = inject(DestroyRef);
    private readonly messageBus = inject(MessageBus);
    private readonly themeService = inject(ThemeService);
    private fetcher?: ReturnType<typeof createGraphiQLFetcher>;
    private reactRoot?: ReactDOM.Root | null;

    protected readonly clientsState = inject(ClientsState);

    protected readonly graphiQLContainer = viewChild.required<ElementRef<HTMLDivElement>>('graphiQLContainer');

    protected readonly clientSelected = signal<ClientDto | null>(null);

    protected readonly accessToken = rxResource({
        params: () => ({ client: this.clientSelected() }),
        stream: ({ params: { client } }) => {
            if (!client) {
                return of(this.authService.user?.accessToken!);
            }

            return this.clientsService.createToken(this.appsState.appName, client).pipe(map(token => token.accessToken));
        },
    });

    protected readonly clientsReadable = this.appsState.snapshot.selectedApp!.canReadClients;

    protected readonly clientsDialog = new DialogModel();

    protected readonly clientForm = form(this.clientSelected);

    constructor() {
        if (this.clientsReadable) {
            this.clientsState.load();
        }

        afterNextRender(() => {
            this.reactRoot = ReactDOM.createRoot(this.graphiQLContainer().nativeElement);
            this.render();
        });

        effect(() => {
            if (!this.accessToken.hasValue()) {
                return;
            }

            const accessToken = this.accessToken.value();

            untracked(() => this.initOrUpdateGraphQL(accessToken));
        });

        effect(() => {
            this.themeService.theme();
            this.render();
        });

        this.destroyRef.onDestroy(() => {
            this.reactRoot?.unmount();
            this.reactRoot = null;
        });
    }

    private initOrUpdateGraphQL(accessToken: string) {
        const graphQLEndpoint = this.apiUrl.buildUrl(`api/content/${this.appsState.appName}/graphql`);

        const subscriptionUrl =
            graphQLEndpoint
                .replace('http://', 'ws://')
                .replace('https://', 'wss://') +
                `?access_token=${accessToken}`;

        this.fetcher = createGraphiQLFetcher({
            url: graphQLEndpoint,
            headers: {
                Authorization: `Bearer ${accessToken}`,
            },
            fetch: (input: any, init?: RequestInit) => {
                const isIntrospection = Types.isString(init?.body) && init!.body.indexOf('IntrospectionQuery') >= 0;

                if (!isIntrospection) {
                    this.messageBus.emit(new QueryExecuted());
                }

                return fetch(input, init);
            },
            subscriptionUrl,
        });

        this.render();
    }

    private render() {
        if (!this.fetcher) {
            return;
        }

        this.reactRoot?.render(
            React.createElement(GraphiQL, {
                fetcher: this.fetcher,
                forcedTheme: this.themeService.theme(),
            }),
        );
    }
}
