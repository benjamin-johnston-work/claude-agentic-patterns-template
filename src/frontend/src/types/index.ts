// Repository Types
export interface Repository {
  id: string;
  name: string;
  url: string;
  language: string;
  description?: string;
  status: RepositoryStatus;
  branches: Branch[];
  statistics?: RepositoryStatistics;
  hasDocumentation: boolean;
  documentationStatus: DocumentationStatus;
  documentationLastGenerated?: string;
  hasKnowledgeGraph: boolean;
  complexityScore?: number;
  architecturalHealth?: number;
  createdAt: string;
  updatedAt: string;
}

export enum RepositoryStatus {
  Connecting = 'CONNECTING',
  Connected = 'CONNECTED',
  Analyzing = 'ANALYZING',
  Ready = 'READY',
  Error = 'ERROR',
  Disconnected = 'DISCONNECTED',
}

export interface Branch {
  name: string;
  isDefault: boolean;
  lastCommit?: Commit;
  createdAt: string;
}

export interface Commit {
  hash: string;
  message: string;
  author: string;
  timestamp: string;
}

export interface RepositoryStatistics {
  fileCount: number;
  lineCount: number;
  languageBreakdown: LanguageStats[];
}

export interface LanguageStats {
  language: string;
  fileCount: number;
  lineCount: number;
  percentage: number;
}

// File System Types
export interface FileStructure {
  folders: Folder[];
  files: FileInfo[];
}

export interface Folder {
  name: string;
  path: string;
  files: FileInfo[];
  subfolders: Folder[];
}

export interface FileInfo {
  name: string;
  path: string;
  size: number;
  language: string;
  lastModified: string;
}

export interface FileContent {
  content: string;
  language: string;
  size: number;
  lastModified: string;
  path: string;
}

// Documentation Types
export interface Documentation {
  id: string;
  title: string;
  status: DocumentationStatus;
  sections: DocumentationSection[];
  metadata: DocumentationMetadata;
  statistics: DocumentationStatistics;
  generatedAt: string;
  createdAt: string;
  updatedAt: string;
}

export enum DocumentationStatus {
  NotStarted = 'NOT_STARTED',
  InProgress = 'IN_PROGRESS',
  Completed = 'COMPLETED',
  Failed = 'FAILED',
}

export interface DocumentationSection {
  id: string;
  title: string;
  content: string;
  type: DocumentationSectionType;
  order: number;
  metadata: SectionMetadata;
}

export enum DocumentationSectionType {
  Overview = 'OVERVIEW',
  Installation = 'INSTALLATION',
  Usage = 'USAGE',
  API = 'API',
  Examples = 'EXAMPLES',
  Configuration = 'CONFIGURATION',
  Troubleshooting = 'TROUBLESHOOTING',
  Contributing = 'CONTRIBUTING',
  License = 'LICENSE',
  Custom = 'CUSTOM',
}

export interface SectionMetadata {
  wordCount: number;
  readingTime: number;
  lastModified: string;
}

export interface DocumentationMetadata {
  totalSections: number;
  totalWords: number;
  estimatedReadingTime: number;
  lastGenerated: string;
  generationDuration: number;
}

export interface DocumentationStatistics {
  sectionsGenerated: number;
  wordsGenerated: number;
  averageSectionLength: number;
}

// Search Types
export interface SearchResults {
  results: SearchResult[];
  totalCount: number;
  searchDuration: number;
  facets: SearchFacet[];
}

export interface SearchResult {
  id: string;
  title: string;
  content: string;
  excerpt: string;
  repositoryId: string;
  repositoryName: string;
  filePath: string;
  language: string;
  score: number;
  highlights: string[];
  lastModified: string;
}

export interface SearchFacet {
  field: string;
  values: FacetValue[];
}

export interface FacetValue {
  value: string;
  count: number;
}

export interface SearchFilter {
  field: string;
  operator: SearchOperator;
  value: string;
}

export enum SearchOperator {
  Equals = 'EQUALS',
  Contains = 'CONTAINS',
  StartsWith = 'STARTS_WITH',
  EndsWith = 'ENDS_WITH',
  GreaterThan = 'GREATER_THAN',
  LessThan = 'LESS_THAN',
}

export enum SearchType {
  Semantic = 'SEMANTIC',
  Keyword = 'KEYWORD',
  Code = 'CODE',
  Documentation = 'DOCUMENTATION',
}

export interface SearchSuggestions {
  suggestions: string[];
  popularQueries: string[];
}

// Conversation Types
export interface Conversation {
  id: string;
  title: string;
  repositoryId?: string;
  repositoryName?: string;
  messages: ChatMessage[];
  messageCount: number;
  lastMessageAt: string;
  metadata: ConversationMetadata;
  createdAt: string;
  updatedAt: string;
}

export interface ChatMessage {
  id: string;
  content: string;
  sender: MessageSender;
  timestamp: string;
  conversationId: string;
  metadata: MessageMetadata;
  repositoryContext?: string;
}

export enum MessageSender {
  User = 'USER',
  Assistant = 'ASSISTANT',
  System = 'SYSTEM',
}

export interface MessageMetadata {
  type: MessageType;
  attachments: MessageAttachment[];
}

export enum MessageType {
  Text = 'TEXT',
  Code = 'CODE',
  File = 'FILE',
  Image = 'IMAGE',
  Link = 'LINK',
  System = 'SYSTEM',
}

export interface MessageAttachment {
  type: AttachmentType;
  url: string;
  title: string;
  description?: string;
}

export enum AttachmentType {
  File = 'FILE',
  Image = 'IMAGE',
  Link = 'LINK',
  Code = 'CODE',
  Repository = 'REPOSITORY',
}

export interface ConversationMetadata {
  tags: string[];
  summary?: string;
  participants: string[];
}

// Knowledge Graph Types
export interface KnowledgeGraph {
  id: string;
  entities: CodeEntity[];
  relationships: CodeRelationship[];
  statistics: GraphStatistics;
  metadata: GraphMetadata;
  status: GraphStatus;
  createdAt: string;
  updatedAt: string;
}

export interface CodeEntity {
  id: string;
  name: string;
  type: EntityType;
  location: EntityLocation;
  metadata: EntityMetadata;
  complexityScore: number;
}

export enum EntityType {
  Class = 'CLASS',
  Interface = 'INTERFACE',
  Method = 'METHOD',
  Property = 'PROPERTY',
  Field = 'FIELD',
  Enum = 'ENUM',
  Namespace = 'NAMESPACE',
  Module = 'MODULE',
  Function = 'FUNCTION',
  Variable = 'VARIABLE',
  Constant = 'CONSTANT',
  Type = 'TYPE',
}

export interface EntityLocation {
  filePath: string;
  lineNumber: number;
  columnNumber: number;
}

export interface EntityMetadata {
  description?: string;
  complexity: number;
  dependencies: string[];
  attributes: EntityAttribute[];
}

export interface EntityAttribute {
  name: string;
  type: string;
  accessModifier: AccessModifier;
}

export enum AccessModifier {
  Public = 'PUBLIC',
  Private = 'PRIVATE',
  Protected = 'PROTECTED',
  Internal = 'INTERNAL',
}

export interface CodeRelationship {
  id: string;
  sourceEntityId: string;
  targetEntityId: string;
  type: RelationshipType;
  metadata: RelationshipMetadata;
}

export enum RelationshipType {
  Inherits = 'INHERITS',
  Implements = 'IMPLEMENTS',
  Contains = 'CONTAINS',
  Uses = 'USES',
  Calls = 'CALLS',
  References = 'REFERENCES',
  Depends = 'DEPENDS',
  Aggregates = 'AGGREGATES',
  Composes = 'COMPOSES',
}

export interface RelationshipMetadata {
  strength: number;
  description?: string;
  sourceLocation?: EntityLocation;
}

export interface GraphStatistics {
  totalEntities: number;
  totalRelationships: number;
  averageComplexity: number;
  entityTypeBreakdown: TypeBreakdown[];
  relationshipTypeBreakdown: TypeBreakdown[];
}

export interface TypeBreakdown {
  type: string;
  count: number;
}

export interface GraphMetadata {
  analysisDate: string;
  version: string;
  repositoryIds: string[];
}

export enum GraphStatus {
  Building = 'BUILDING',
  Ready = 'READY',
  Updating = 'UPDATING',
  Error = 'ERROR',
}

// Architectural Pattern Types
export interface ArchitecturalPattern {
  id: string;
  type: PatternType;
  name: string;
  description: string;
  confidence: number;
  hasViolations: boolean;
  hasCriticalViolations: boolean;
  metadata: PatternMetadata;
  entities: CodeEntity[];
}

export enum PatternType {
  MVC = 'MVC',
  Repository = 'REPOSITORY',
  Factory = 'FACTORY',
  Singleton = 'SINGLETON',
  Observer = 'OBSERVER',
  Strategy = 'STRATEGY',
  Decorator = 'DECORATOR',
  Adapter = 'ADAPTER',
  Facade = 'FACADE',
  Proxy = 'PROXY',
  Command = 'COMMAND',
  State = 'STATE',
  Template = 'TEMPLATE',
  Bridge = 'BRIDGE',
  Composite = 'COMPOSITE',
  Flyweight = 'FLYWEIGHT',
  Prototype = 'PROTOTYPE',
  Builder = 'BUILDER',
  AbstractFactory = 'ABSTRACT_FACTORY',
  ChainOfResponsibility = 'CHAIN_OF_RESPONSIBILITY',
  Interpreter = 'INTERPRETER',
  Iterator = 'ITERATOR',
  Mediator = 'MEDIATOR',
  Memento = 'MEMENTO',
  Visitor = 'VISITOR',
  Custom = 'CUSTOM',
}

export interface PatternMetadata {
  detectedAt: string;
  pattern: string;
  antiPatterns: AntiPattern[];
  violations: PatternViolation[];
}

export interface AntiPattern {
  type: string;
  description: string;
  severity: ViolationSeverity;
  location: EntityLocation;
}

export interface PatternViolation {
  type: string;
  description: string;
  severity: ViolationSeverity;
  location: EntityLocation;
}

export enum ViolationSeverity {
  Low = 'LOW',
  Medium = 'MEDIUM',
  High = 'HIGH',
  Critical = 'CRITICAL',
}

// UI State Types
export interface NavigationState {
  sidebarOpen: boolean;
  mobileMenuOpen: boolean;
  currentPath: string;
}

export interface LoadingState {
  isLoading: boolean;
  message?: string;
  progress?: number;
}

export interface ErrorState {
  hasError: boolean;
  message?: string;
  details?: string;
  code?: string;
}

export interface ToastMessage {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  description?: string;
  duration?: number;
}

// Form Types
export interface AddRepositoryInput {
  url: string;
  accessToken?: string;
  branch?: string;
}

export interface SearchRepositoriesInput {
  query: string;
  searchType: SearchType;
  filters?: SearchFilter[];
  top?: number;
  skip?: number;
}

export interface StartConversationInput {
  repositoryId?: string;
  initialMessage: string;
  title?: string;
}

export interface SendMessageInput {
  conversationId: string;
  content: string;
  type?: MessageType;
  attachments?: MessageAttachment[];
}

export interface ProcessQueryInput {
  query: string;
  repositoryId?: string;
  conversationId?: string;
  context?: string;
}

// Filter and Pagination Types
export interface RepositoryFilter {
  name?: string;
  language?: string;
  status?: RepositoryStatus;
  hasDocumentation?: boolean;
  hasKnowledgeGraph?: boolean;
}

export interface PaginationInfo {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// Component Props Types
export interface ComponentBaseProps {
  className?: string;
  children?: React.ReactNode;
}

export interface WithLoadingProps {
  loading?: boolean;
  error?: Error | null;
}

export interface WithPaginationProps {
  pagination?: PaginationInfo;
  onPageChange?: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
}

// API Response Types
export interface ApiResponse<T> {
  data?: T;
  error?: ApiError;
  loading: boolean;
}

export interface ApiError {
  message: string;
  code?: string;
  details?: string;
  timestamp: string;
}