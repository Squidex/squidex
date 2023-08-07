/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject, InjectionToken } from '@angular/core';
import { filter, Observable, take } from 'rxjs';
import { MessageBus, StepDefinition, waitForAnchor } from '@app/framework';
import { ClientTourStated, QueryExecuted } from './../utils/messages';
import { AppsState } from './apps.state';
import { AssetsState } from './assets.state';
import { ContentsState } from './contents.state';
import { SchemasState } from './schemas.state';

export const TASK_CONFIGURATION = new InjectionToken<TaskConfiguration>('Tasks');

export interface TaskConfiguration {
    defaults: TaskDefinition;

    // The actual tasks.
    tasks: TaskDefinition[];
}

export interface TaskDefinition {
    // The ID to store it in the backend.
    id: string;

    // The title.
    title: string;

    // The description.
    description: string;

    // The steps to operate.
    steps: StepDefinition[];

    // Indicates that the task has been completed.
    onComplete: Observable<any>;
}

export function buildTasks() {
   const tasks: TaskDefinition[] = [{
        id: 'createApp',
        title: 'i18n:tour.createApp.title',
        description: 'i18n:tour.createApp.description',
        steps: [{
            anchorId: 'addApp',
            content: 'i18n:tour.createApp.buttonContent',
            nextOnAnchorClick: true,
            scrollContainer: '.panel-container',
            position: 'top-left',
        }, {
            anchorId: 'appForm',
            content: 'i18n:tour.createApp.formContent',
            nextOnCondition: waitForAnchor('app'),
            scrollContainer: '.panel-container',
            position: 'left-top',
        }, {
            anchorId: 'app',
            content: 'i18n:tour.general.selectAppContent',
            nextOnAnchorClick: true,
            scrollContainer: '.panel-container',
        }, {
            anchorId: 'appDashboard',
            content: 'i18n:tour.createApp.dashboardContent',
            isAsync: true,
            position: 'right-top',
        }],
        onComplete: (() => {
            const appsState = inject(AppsState);

            return appsState.changes.pipe(
                filter(change =>
                    change.snapshot.apps.length > 0),
                take(1));
        })(),
    }, {
        id: 'createSchema',
        title: 'i18n:tour.createSchema.title',
        description: 'i18n:tour.createSchema.description',
        steps: [{
            anchorId: 'app',
            content: 'i18n:tour.general.selectAppContent',
            isOptional: true,
            nextOnAnchorClick: true,
        }, {
            anchorId: 'schemas',
            content: 'i18n:tour.createSchema.navigateContent',
            nextOnAnchorClick: true,
            position: 'right-center',
        }, {
            anchorId: 'addSchema',
            content: 'i18n:tour.createSchema.addContent',
            isAsync: true,
            nextOnAnchorClick: true,
        }, {
            anchorId: 'schemaForm',
            content: 'i18n:tour.createSchema.formContent',
            nextOnCondition: waitForAnchor('schema'),
        }, {
            anchorId: 'addField',
            content: 'i18n:tour.createSchema.addFieldContent',
            isAsync: true,
            nextOnAnchorClick: true,
            scrollContainer: '.list-content',
        }, {
            anchorId: 'fieldForm',
            content: 'i18n:tour.createSchema.fieldFormContent',
            nextOnCondition: waitForAnchor('schemaField'),
            position: ['right-to-right', 'top-to-top'],
        }, {
            anchorId: 'schemaField',
            content: 'i18n:tour.createSchema.schemaFieldContent',
            scrollContainer: '.list-content',
        }, {
            anchorId: 'publishSchema',
            content: 'i18n:tour.createSchema.publishContent',
            nextOnAnchorClick: true,
        }, {
            anchorId: 'help',
            content: 'i18n:tour.createSchema.helpContent',
            scrollContainer: '.panel-container',
            position: 'left-center',
        }, {
            anchorId: 'history',
            content: 'i18n:tour.createSchema.historyContent',
            scrollContainer: '.panel-container',
            position: 'left-center',
        }],
        onComplete: (() => {
            const schemasState = inject(SchemasState);

            return schemasState.changes.pipe(
                filter(change =>
                    change.snapshot.schemas.find(s => s.fields.length > 0 && s.isPublished) !== undefined),
                take(1));
        })(),
    }, {
        id: 'createContent',
        title: 'i18n:tour.createContent.title',
        description: 'i18n:tour.createContent.description',
        steps: [{
            anchorId: 'app',
            content: 'i18n:tour.general.selectAppContent',
            isOptional: true,
            nextOnAnchorClick: true,
            scrollContainer: '.panel-container',
        }, {
            anchorId: 'content',
            content: 'i18n:tour.createContent.navigateContent',
            nextOnAnchorClick: true,
            position: 'right-center',
        }, {
            anchorId: 'schema',
            content: 'i18n:tour.createContent.selectSchemaContent',
            nextOnAnchorClick: true,
        }, {
            anchorId: 'addContent',
            content: 'i18n:tour.createContent.addContent',
            nextOnAnchorClick: true,
            isAsync: true,
            position: 'bottom-right',
        }, {
            anchorId: 'saveContent',
            content: 'i18n:tour.createContent.saveContent',
            nextOnAnchorClick: true,
            isAsync: true,
            position: 'left-top',
            enableBackdrop: false,
        }, {
            anchorId: 'status',
            content: 'i18n:tour.createContent.statusContent',
            isAsync: true,
            position: 'left-center',
        }],
        onComplete: (() => {
            const contentsState = inject(ContentsState);

            return contentsState.changes.pipe(
                filter(change =>
                    change.snapshot.contents.find(s => s.status === 'Published') !== undefined ||
                    change.snapshot.selectedContent?.status === 'Published'),
                take(1));
        })(),
    }, {
        id: 'createAsset',
        title: 'i18n:tour.createAsset.title',
        description: 'i18n:tour.createContent.description',
        steps: [{
            anchorId: 'app',
            content: 'i18n:tour.general.selectAppContent',
            isOptional: true,
            nextOnAnchorClick: true,
            scrollContainer: '.panel-container',
        }, {
            anchorId: 'assets',
            content: 'i18n:tour.createAsset.navigateContent',
            nextOnAnchorClick: true,
            position: 'right-center',
        }, {
            anchorId: 'upload',
            content: 'i18n:tour.createAsset.uploadContent',
            isAsync: true,
        }, {
            anchorId: 'filter',
            content: 'i18n:tour.createAsset.filterContent',
            position: 'left-center',
        }],
        onComplete: (() => {
            const assetsState = inject(AssetsState);

            return assetsState.changes.pipe(
                filter(change =>
                    change.snapshot.assets.length > 0),
                take(1));
        })(),
    }, {
        id: 'checkClient',
        title: 'i18n:tour.checkClient.title',
        description: 'i18n:tour.checkClient.description',
        steps: [{
            anchorId: 'app',
            content: 'i18n:tour.general.selectAppContent',
            isOptional: true,
            nextOnAnchorClick: true,
        }, {
            anchorId: 'settings',
            content: 'i18n:tour.checkClient.navigateContent',
            nextOnAnchorClick: true,
            position: 'right-bottom',
        }, {
            anchorId: 'clients',
            content: 'i18n:tour.checkClient.navigateClientsContent',
            isAsync: true,
            nextOnAnchorClick: true,
        }, {
            anchorId: 'client',
            content: 'i18n:tour.checkClient.clientContent',
            isAsync: true,
            scrollContainer: '.list-content',
        }, {
            anchorId: 'connect',
            content: 'i18n:tour.checkClient.connectContent',
            nextOnAnchorClick: true,
        }],
        onComplete: (() => {
            const messageBus = inject(MessageBus);

            return messageBus.of(ClientTourStated).pipe(take(1));
        })(),
    }, {
        id: 'testGraphQL',
        title: 'i18n:tour.testGraphQL.title',
        description: 'i18n:tour.testGraphQL.description',
        steps: [{
            anchorId: 'app',
            content: 'i18n:tour.general.selectAppContent',
            isOptional: true,
            nextOnAnchorClick: true,
            scrollContainer: '.panel-container',
        }, {
            anchorId: 'api',
            content: 'i18n:tour.testGraphQL.navigateContent',
            nextOnAnchorClick: true,
            position: 'right-center',
        }, {
            anchorId: 'graphql',
            content: 'i18n:tour.testGraphQL.navigateGraphQLContent',
            isAsync: true,
            nextOnAnchorClick: true,
        }, {
            anchorId: 'graphQLExplorer',
            content: 'i18n:tour.testGraphQL.queryContent',
            enableBackdrop: false,
        }],
        onComplete: (() => {
            const messageBus = inject(MessageBus);

            return messageBus.of(QueryExecuted).pipe(take(1));
        })(),
    }];

    const defaults: StepDefinition = {
        allowUserInitiatedNavigation: true,
        enableBackdrop: true,
        disablePageScrolling: true,
        disableScrollToAnchor: false,
        duplicateAnchorHandling: 'registerLast',
    };

    return { tasks, defaults };
}