--
-- PostgreSQL database dump
--

\restrict OY2lIdB0b3X6Tbv4GLbGrkVlsJTL9GSeTvPwM5or4Z4VmKnpfdvjW9Gw9cnkOMb

-- Dumped from database version 17.6 (Debian 17.6-2.pgdg13+1)
-- Dumped by pg_dump version 17.6 (Debian 17.6-2.pgdg13+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: OpenIddictApplications; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."OpenIddictApplications" (
    id text NOT NULL,
    application_type character varying(50),
    client_id character varying(100),
    client_secret text,
    client_type character varying(50),
    concurrency_token character varying(50),
    consent_type character varying(50),
    display_name text,
    display_names text,
    json_web_key_set text,
    permissions text,
    post_logout_redirect_uris text,
    properties text,
    redirect_uris text,
    requirements text,
    settings text
);


--
-- Name: OpenIddictAuthorizations; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."OpenIddictAuthorizations" (
    id text NOT NULL,
    application_id text,
    concurrency_token character varying(50),
    creation_date timestamp with time zone,
    properties text,
    scopes text,
    status character varying(50),
    subject character varying(400),
    type character varying(50)
);


--
-- Name: OpenIddictScopes; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."OpenIddictScopes" (
    id text NOT NULL,
    concurrency_token character varying(50),
    description text,
    descriptions text,
    display_name text,
    display_names text,
    name character varying(200),
    properties text,
    resources text
);


--
-- Name: OpenIddictTokens; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."OpenIddictTokens" (
    id text NOT NULL,
    application_id text,
    authorization_id text,
    concurrency_token character varying(50),
    creation_date timestamp with time zone,
    expiration_date timestamp with time zone,
    payload text,
    properties text,
    redemption_date timestamp with time zone,
    reference_id character varying(100),
    status character varying(50),
    subject character varying(400),
    type character varying(50)
);


--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL
);


--
-- Name: api_key_usage_logs; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.api_key_usage_logs (
    id uuid NOT NULL,
    api_key_id uuid NOT NULL,
    "timestamp" timestamp with time zone NOT NULL,
    method character varying(10) NOT NULL,
    endpoint character varying(500) NOT NULL,
    status_code integer NOT NULL,
    response_time integer DEFAULT 0 NOT NULL,
    ip_address character varying(45) NOT NULL,
    user_agent character varying(1000),
    request_size bigint DEFAULT 0 NOT NULL,
    response_size bigint DEFAULT 0 NOT NULL,
    error_message character varying(1000),
    request_id character varying(100),
    scopes_used text NOT NULL,
    created_at timestamp with time zone,
    updated_at timestamp with time zone,
    create_by text,
    update_by text,
    is_deleted text
);


--
-- Name: api_keys; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.api_keys (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    name character varying(100) NOT NULL,
    key_hash character varying(255) NOT NULL,
    scopes text NOT NULL,
    status integer NOT NULL,
    expires_at timestamp with time zone,
    last_used_at timestamp with time zone,
    usage_count bigint DEFAULT 0 NOT NULL,
    created_at timestamp with time zone,
    updated_at timestamp with time zone,
    create_by text,
    update_by text,
    is_deleted text,
    description character varying(500),
    key_prefix character varying(32) DEFAULT ''::character varying NOT NULL,
    daily_usage_quota integer DEFAULT 10000 NOT NULL,
    ip_whitelist text DEFAULT ''::text NOT NULL,
    last_reset_date timestamp with time zone,
    rate_limit_per_minute integer DEFAULT 100 NOT NULL,
    revoked_at timestamp with time zone,
    security_settings text DEFAULT ''::text NOT NULL,
    today_usage_count integer DEFAULT 0 NOT NULL
);


--
-- Name: o_auth_clients; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.o_auth_clients (
    id uuid NOT NULL,
    client_id character varying(100) NOT NULL,
    client_secret_hash character varying(255),
    name character varying(200) NOT NULL,
    description character varying(500),
    type integer NOT NULL,
    platform integer NOT NULL,
    redirect_uris character varying(2000) NOT NULL,
    post_logout_redirect_uris character varying(2000),
    allowed_scopes character varying(1000) NOT NULL,
    is_active boolean NOT NULL,
    access_token_lifetime integer NOT NULL,
    refresh_token_lifetime integer NOT NULL,
    allow_refresh_tokens boolean NOT NULL,
    require_pkce boolean NOT NULL,
    application_url character varying(500),
    created_at timestamp with time zone,
    updated_at timestamp with time zone,
    create_by text,
    update_by text,
    is_deleted text
);


--
-- Name: refresh_tokens; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.refresh_tokens (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    token character varying(255) NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    is_revoked boolean NOT NULL,
    revoked_by character varying(100),
    revoked_at timestamp with time zone,
    created_at timestamp with time zone,
    updated_at timestamp with time zone,
    create_by text,
    update_by text,
    is_deleted text
);


--
-- Name: roles; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.roles (
    id uuid NOT NULL,
    name character varying(50) NOT NULL,
    description character varying(200) NOT NULL,
    permissions text NOT NULL,
    created_at timestamp with time zone,
    updated_at timestamp with time zone,
    create_by text,
    update_by text,
    is_deleted text
);


--
-- Name: user_logins; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.user_logins (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    provider character varying(50) NOT NULL,
    provider_user_id character varying(100) NOT NULL,
    provider_display_name character varying(200),
    last_login_at timestamp with time zone,
    created_at timestamp with time zone,
    updated_at timestamp with time zone,
    create_by text,
    update_by text,
    is_deleted text
);


--
-- Name: user_roles; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.user_roles (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    role_id uuid NOT NULL,
    assigned_at timestamp with time zone NOT NULL,
    assigned_by uuid,
    created_at timestamp with time zone,
    updated_at timestamp with time zone,
    create_by text,
    update_by text,
    is_deleted text
);


--
-- Name: users; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.users (
    id uuid NOT NULL,
    email character varying(256) NOT NULL,
    username character varying(50) NOT NULL,
    password_hash character varying(255) NOT NULL,
    full_name character varying(200) NOT NULL,
    avatar_url character varying(500),
    is_active boolean NOT NULL,
    email_confirmed boolean DEFAULT false NOT NULL,
    google_id character varying(100),
    last_login_at timestamp with time zone,
    created_at timestamp with time zone,
    updated_at timestamp with time zone,
    create_by text,
    update_by text,
    is_deleted text,
    name character varying(200) DEFAULT ''::character varying NOT NULL,
    picture_url character varying(500)
);


--
-- Data for Name: OpenIddictApplications; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."OpenIddictApplications" (id, application_type, client_id, client_secret, client_type, concurrency_token, consent_type, display_name, display_names, json_web_key_set, permissions, post_logout_redirect_uris, properties, redirect_uris, requirements, settings) FROM stdin;
0d272d30-4c01-46c4-ae6a-479ddf01f5a4	\N	thm2-legacy-client	\N	public	8e7485de-2a26-4b9c-9b32-1615e896d026	\N	Legacy	\N	\N	["ept:token","gt:refresh_token"]	\N	\N	\N	\N	\N
\.


--
-- Data for Name: OpenIddictAuthorizations; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."OpenIddictAuthorizations" (id, application_id, concurrency_token, creation_date, properties, scopes, status, subject, type) FROM stdin;
\.


--
-- Data for Name: OpenIddictScopes; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."OpenIddictScopes" (id, concurrency_token, description, descriptions, display_name, display_names, name, properties, resources) FROM stdin;
\.


--
-- Data for Name: OpenIddictTokens; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."OpenIddictTokens" (id, application_id, authorization_id, concurrency_token, creation_date, expiration_date, payload, properties, redemption_date, reference_id, status, subject, type) FROM stdin;
81bb7e12-4729-4d0d-9ea6-0c475772fb27	0d272d30-4c01-46c4-ae6a-479ddf01f5a4	\N	bc0a7a12-6c95-437e-a42e-13fd1f6200fa	2026-10-05 14:29:00.746044+00	2026-10-06 14:29:00.746063+00	\N	\N	\N	\N	valid	thm2-legacy-subject	access_token
b2a6184c-9412-41b1-aae9-e0627c8d5afb	0d272d30-4c01-46c4-ae6a-479ddf01f5a4	\N	79ef37de-3ee0-4a9d-9b4e-0d5e5d3be42e	2026-10-05 14:29:00.800026+00	2026-10-06 14:29:00.800027+00	\N	\N	\N	\N	valid	thm2-legacy-subject	refresh_token
\.


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."__EFMigrationsHistory" (migration_id, product_version) FROM stdin;
20250622090100_InitialCreate	9.0.5
20250622153945_ConsolidateDbContextAndAddUserLogin	9.0.5
20250706044938_FixPendingModelChanges	9.0.5
20250706135843_EnhancedApiKeyManagement	9.0.5
\.


--
-- Data for Name: api_key_usage_logs; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.api_key_usage_logs (id, api_key_id, "timestamp", method, endpoint, status_code, response_time, ip_address, user_agent, request_size, response_size, error_message, request_id, scopes_used, created_at, updated_at, create_by, update_by, is_deleted) FROM stdin;
\.


--
-- Data for Name: api_keys; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.api_keys (id, user_id, name, key_hash, scopes, status, expires_at, last_used_at, usage_count, created_at, updated_at, create_by, update_by, is_deleted, description, key_prefix, daily_usage_quota, ip_whitelist, last_reset_date, rate_limit_per_minute, revoked_at, security_settings, today_usage_count) FROM stdin;
\.


--
-- Data for Name: o_auth_clients; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.o_auth_clients (id, client_id, client_secret_hash, name, description, type, platform, redirect_uris, post_logout_redirect_uris, allowed_scopes, is_active, access_token_lifetime, refresh_token_lifetime, allow_refresh_tokens, require_pkce, application_url, created_at, updated_at, create_by, update_by, is_deleted) FROM stdin;
44444444-4444-4444-4444-444444444444	tihomo-mobile-ios	\N	TiHoMo iOS Application	iOS mobile application client	0	1	tihomo://auth/callback,tihomo-ios://auth/callback	tihomo://logout,tihomo-ios://logout	openid,profile,email,offline_access	t	3600	2592000	t	t	\N	2024-01-01 00:00:00+00	2024-01-01 00:00:00+00	\N	\N	\N
55555555-5555-5555-5555-555555555555	tihomo-mobile-android	\N	TiHoMo Android Application	Android mobile application client	0	2	tihomo://auth/callback,tihomo-android://auth/callback	tihomo://logout,tihomo-android://logout	openid,profile,email,offline_access	t	3600	2592000	t	t	\N	2024-01-01 00:00:00+00	2024-01-01 00:00:00+00	\N	\N	\N
33333333-3333-3333-3333-333333333333	tihomo-web-client	\N	TiHoMo Web Application	Nuxt.js web application client	0	0	http://localhost:3500/auth/callback,https://app.tihomo.vn/auth/callback	http://localhost:3500,https://app.tihomo.vn	openid,profile,email,offline_access	t	3600	2592000	t	t	https://app.tihomo.vn	2024-01-01 00:00:00+00	2024-01-01 00:00:00+00	\N	\N	\N
\.


--
-- Data for Name: refresh_tokens; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.refresh_tokens (id, user_id, token, expires_at, is_revoked, revoked_by, revoked_at, created_at, updated_at, create_by, update_by, is_deleted) FROM stdin;
\.


--
-- Data for Name: roles; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.roles (id, name, description, permissions, created_at, updated_at, create_by, update_by, is_deleted) FROM stdin;
11111111-1111-1111-1111-111111111111	User	Standard user role	["read:profile","update:profile"]	2024-01-01 00:00:00+00	2024-01-01 00:00:00+00	\N	\N	\N
22222222-2222-2222-2222-222222222222	Admin	Administrator role	["*"]	2024-01-01 00:00:00+00	2024-01-01 00:00:00+00	\N	\N	\N
\.


--
-- Data for Name: user_logins; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.user_logins (id, user_id, provider, provider_user_id, provider_display_name, last_login_at, created_at, updated_at, create_by, update_by, is_deleted) FROM stdin;
\.


--
-- Data for Name: user_roles; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.user_roles (id, user_id, role_id, assigned_at, assigned_by, created_at, updated_at, create_by, update_by, is_deleted) FROM stdin;
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.users (id, email, username, password_hash, full_name, avatar_url, is_active, email_confirmed, google_id, last_login_at, created_at, updated_at, create_by, update_by, is_deleted, name, picture_url) FROM stdin;
\.


--
-- Name: __EFMigrationsHistory pk___ef_migrations_history; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id);


--
-- Name: api_key_usage_logs pk_api_key_usage_logs; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.api_key_usage_logs
    ADD CONSTRAINT pk_api_key_usage_logs PRIMARY KEY (id);


--
-- Name: api_keys pk_api_keys; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.api_keys
    ADD CONSTRAINT pk_api_keys PRIMARY KEY (id);


--
-- Name: o_auth_clients pk_o_auth_clients; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.o_auth_clients
    ADD CONSTRAINT pk_o_auth_clients PRIMARY KEY (id);


--
-- Name: OpenIddictApplications pk_open_iddict_applications; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."OpenIddictApplications"
    ADD CONSTRAINT pk_open_iddict_applications PRIMARY KEY (id);


--
-- Name: OpenIddictAuthorizations pk_open_iddict_authorizations; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."OpenIddictAuthorizations"
    ADD CONSTRAINT pk_open_iddict_authorizations PRIMARY KEY (id);


--
-- Name: OpenIddictScopes pk_open_iddict_scopes; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."OpenIddictScopes"
    ADD CONSTRAINT pk_open_iddict_scopes PRIMARY KEY (id);


--
-- Name: OpenIddictTokens pk_open_iddict_tokens; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."OpenIddictTokens"
    ADD CONSTRAINT pk_open_iddict_tokens PRIMARY KEY (id);


--
-- Name: refresh_tokens pk_refresh_tokens; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.refresh_tokens
    ADD CONSTRAINT pk_refresh_tokens PRIMARY KEY (id);


--
-- Name: roles pk_roles; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT pk_roles PRIMARY KEY (id);


--
-- Name: user_logins pk_user_logins; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_logins
    ADD CONSTRAINT pk_user_logins PRIMARY KEY (id);


--
-- Name: user_roles pk_user_roles; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_roles
    ADD CONSTRAINT pk_user_roles PRIMARY KEY (id);


--
-- Name: users pk_users; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT pk_users PRIMARY KEY (id);


--
-- Name: ix_api_key_usage_logs_api_key_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_api_key_usage_logs_api_key_id ON public.api_key_usage_logs USING btree (api_key_id);


--
-- Name: ix_api_key_usage_logs_api_key_id_timestamp; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_api_key_usage_logs_api_key_id_timestamp ON public.api_key_usage_logs USING btree (api_key_id, "timestamp");


--
-- Name: ix_api_key_usage_logs_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_api_key_usage_logs_id ON public.api_key_usage_logs USING btree (id);


--
-- Name: ix_api_key_usage_logs_ip_address; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_api_key_usage_logs_ip_address ON public.api_key_usage_logs USING btree (ip_address);


--
-- Name: ix_api_key_usage_logs_method; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_api_key_usage_logs_method ON public.api_key_usage_logs USING btree (method);


--
-- Name: ix_api_key_usage_logs_status_code; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_api_key_usage_logs_status_code ON public.api_key_usage_logs USING btree (status_code);


--
-- Name: ix_api_key_usage_logs_timestamp; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_api_key_usage_logs_timestamp ON public.api_key_usage_logs USING btree ("timestamp");


--
-- Name: ix_api_keys_expires_at; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_api_keys_expires_at ON public.api_keys USING btree (expires_at);


--
-- Name: ix_api_keys_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_api_keys_id ON public.api_keys USING btree (id);


--
-- Name: ix_api_keys_key_hash; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_api_keys_key_hash ON public.api_keys USING btree (key_hash);


--
-- Name: ix_api_keys_key_prefix; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_api_keys_key_prefix ON public.api_keys USING btree (key_prefix);


--
-- Name: ix_api_keys_status; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_api_keys_status ON public.api_keys USING btree (status);


--
-- Name: ix_api_keys_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_api_keys_user_id ON public.api_keys USING btree (user_id);


--
-- Name: ix_o_auth_clients_client_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_o_auth_clients_client_id ON public.o_auth_clients USING btree (client_id);


--
-- Name: ix_o_auth_clients_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_o_auth_clients_id ON public.o_auth_clients USING btree (id);


--
-- Name: ix_open_iddict_applications_client_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_open_iddict_applications_client_id ON public."OpenIddictApplications" USING btree (client_id);


--
-- Name: ix_open_iddict_authorizations_application_id_status_subject_typ; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_open_iddict_authorizations_application_id_status_subject_typ ON public."OpenIddictAuthorizations" USING btree (application_id, status, subject, type);


--
-- Name: ix_open_iddict_scopes_name; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_open_iddict_scopes_name ON public."OpenIddictScopes" USING btree (name);


--
-- Name: ix_open_iddict_tokens_application_id_status_subject_type; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_open_iddict_tokens_application_id_status_subject_type ON public."OpenIddictTokens" USING btree (application_id, status, subject, type);


--
-- Name: ix_open_iddict_tokens_authorization_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_open_iddict_tokens_authorization_id ON public."OpenIddictTokens" USING btree (authorization_id);


--
-- Name: ix_open_iddict_tokens_reference_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_open_iddict_tokens_reference_id ON public."OpenIddictTokens" USING btree (reference_id);


--
-- Name: ix_refresh_tokens_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_refresh_tokens_id ON public.refresh_tokens USING btree (id);


--
-- Name: ix_refresh_tokens_token; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_refresh_tokens_token ON public.refresh_tokens USING btree (token);


--
-- Name: ix_refresh_tokens_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_refresh_tokens_user_id ON public.refresh_tokens USING btree (user_id);


--
-- Name: ix_roles_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_roles_id ON public.roles USING btree (id);


--
-- Name: ix_roles_name; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_roles_name ON public.roles USING btree (name);


--
-- Name: ix_user_logins_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_user_logins_id ON public.user_logins USING btree (id);


--
-- Name: ix_user_logins_provider_provider_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_user_logins_provider_provider_user_id ON public.user_logins USING btree (provider, provider_user_id);


--
-- Name: ix_user_logins_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_user_logins_user_id ON public.user_logins USING btree (user_id);


--
-- Name: ix_user_roles_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_user_roles_id ON public.user_roles USING btree (id);


--
-- Name: ix_user_roles_role_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_user_roles_role_id ON public.user_roles USING btree (role_id);


--
-- Name: ix_user_roles_user_id_role_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_user_roles_user_id_role_id ON public.user_roles USING btree (user_id, role_id);


--
-- Name: ix_users_email; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_users_email ON public.users USING btree (email);


--
-- Name: ix_users_google_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_users_google_id ON public.users USING btree (google_id);


--
-- Name: ix_users_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_users_id ON public.users USING btree (id);


--
-- Name: ix_users_username; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_users_username ON public.users USING btree (username);


--
-- Name: api_key_usage_logs fk_api_key_usage_logs_api_keys_api_key_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.api_key_usage_logs
    ADD CONSTRAINT fk_api_key_usage_logs_api_keys_api_key_id FOREIGN KEY (api_key_id) REFERENCES public.api_keys(id) ON DELETE CASCADE;


--
-- Name: api_keys fk_api_keys_users_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.api_keys
    ADD CONSTRAINT fk_api_keys_users_user_id FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: OpenIddictAuthorizations fk_open_iddict_authorizations_open_iddict_applications_applicat; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."OpenIddictAuthorizations"
    ADD CONSTRAINT fk_open_iddict_authorizations_open_iddict_applications_applicat FOREIGN KEY (application_id) REFERENCES public."OpenIddictApplications"(id);


--
-- Name: OpenIddictTokens fk_open_iddict_tokens_open_iddict_applications_application_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."OpenIddictTokens"
    ADD CONSTRAINT fk_open_iddict_tokens_open_iddict_applications_application_id FOREIGN KEY (application_id) REFERENCES public."OpenIddictApplications"(id);


--
-- Name: OpenIddictTokens fk_open_iddict_tokens_open_iddict_authorizations_authorization_; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."OpenIddictTokens"
    ADD CONSTRAINT fk_open_iddict_tokens_open_iddict_authorizations_authorization_ FOREIGN KEY (authorization_id) REFERENCES public."OpenIddictAuthorizations"(id);


--
-- Name: refresh_tokens fk_refresh_tokens_users_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.refresh_tokens
    ADD CONSTRAINT fk_refresh_tokens_users_user_id FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: user_logins fk_user_logins_users_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_logins
    ADD CONSTRAINT fk_user_logins_users_user_id FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: user_roles fk_user_roles_roles_role_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_roles
    ADD CONSTRAINT fk_user_roles_roles_role_id FOREIGN KEY (role_id) REFERENCES public.roles(id) ON DELETE CASCADE;


--
-- Name: user_roles fk_user_roles_users_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_roles
    ADD CONSTRAINT fk_user_roles_users_user_id FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

\unrestrict OY2lIdB0b3X6Tbv4GLbGrkVlsJTL9GSeTvPwM5or4Z4VmKnpfdvjW9Gw9cnkOMb

