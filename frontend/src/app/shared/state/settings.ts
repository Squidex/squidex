/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export const Settings = {
    AppProperties: {
        HIDE_API: 'ui.api.hide',
        HIDE_ASSETS: 'ui.assets.hide',
        HIDE_CONTENTS: (schema: any) => `ui.contents.${schema}.hide`,
        HIDE_SCHEMAS: 'ui.schemas.hide',
        HIDE_SETTINGS: 'ui.settings.hide',
    },
    Local: {
        ASSETS_MODE: 'squidex.assets.list-view',
        CONTENT_LANGUAGE: (schema: any) => `squidex.schemas.${schema}.language`,
        CONTENT_ID_INPUT: 'squidex.contents.idField',
        DASHBOARD_CHART_STACKED: 'dashboard.charts.stacked',
        DISABLE_ONBOARDING: (key: any) => `squidex.onboarding.disable.${key}`,
        FIELD_ALL: (schema: any, field: any) => `squidex.schemas.${schema}.fields.${field}.show-all`,
        FIELD_COLLAPSED: (schema: any, field: any) => `squidex.schemas.${schema}.fields.${field}.closed`,
        FIELD_EDITOR_COLLAPSED: (schema: any, field: any) => `squidex.schemas.${schema}.editor.fields.${field}.closed`,
        HIDE_MAP: 'hideMap',
        NEWS_VERSION: 'squidex.news.version',
        NOTIFICATION_VERSION: 'notifications.version',
        SCHEMA_CATEGORY_COLLAPSED: (category: any) => `squidex.schema.category.${category}.collapsed`,
        SCHEMA_PREVIEW: (schema: any) => `squidex.schemas.${schema}.preview-button`,
        SCHEMAS_COLLAPSED: 'content.schemas.collapsed',
    },
};
