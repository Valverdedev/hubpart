import { Component, signal, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

interface Plano {
  id: string;
  nome: string;
  descricao: string;
  preco: number | null;
  precoAnual: number | null;
  badge: string | null;
  destaque: boolean;
  recursos: string[];
  ctaTexto: string;
}

interface FaqItem {
  pergunta: string;
  resposta: string;
  aberto: boolean;
}

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss',
})
export class LandingComponent {
  /** Navbar scroll state */
  readonly navScrolled = signal(false);

  /** Secao Como Funciona - tab ativa */
  readonly tabAtiva = signal<'compradores' | 'fornecedores'>('compradores');

  /** Ciclo de preços */
  readonly cicloAnual = signal(false);

  /** FAQ accordion */
  readonly faqItems = signal<FaqItem[]>([
    {
      pergunta: 'O AutoPartsHub cobra comissão sobre as vendas?',
      resposta:
        'Não cobramos comissão sobre a transação. O pagamento é feito diretamente entre comprador e vendedor. Nosso modelo é baseado em planos de assinatura de uso da plataforma.',
      aberto: true,
    },
    {
      pergunta: 'Quais tipos de peças posso cotar?',
      resposta:
        'Cotamos peças para automóveis, motos, caminhões e máquinas pesadas — desde filtros e freios até componentes de motor e suspensão. Nossa base cobre mais de 200.000 referências.',
      aberto: false,
    },
    {
      pergunta: 'Preciso instalar algum software?',
      resposta:
        'Não. A plataforma é 100% web e também disponível como PWA (Progressive Web App) para iOS e Android, sem necessidade de instalação pela App Store.',
      aberto: false,
    },
    {
      pergunta: 'Como os fornecedores são avaliados?',
      resposta:
        'Cada transação gera uma avaliação automática de prazo, qualidade da peça e atendimento. Fornecedores com baixa reputação são suspensos automaticamente.',
      aberto: false,
    },
    {
      pergunta: 'A plataforma funciona para motos e pesados?',
      resposta:
        'Sim. Atuamos em todas as categorias: passeio, motos, comerciais leves, caminhões, ônibus e máquinas agrícolas.',
      aberto: false,
    },
    {
      pergunta: 'Existe app para iOS e Android?',
      resposta:
        'Disponibilizamos um PWA instalável que funciona nativamente em iOS e Android. Apps nativos estão no roadmap para 2025.',
      aberto: false,
    },
  ]);

  readonly planos: Plano[] = [
    {
      id: 'starter',
      nome: 'Starter',
      descricao: 'Para oficinas pequenas e novos negócios.',
      preco: null,
      precoAnual: null,
      badge: null,
      destaque: false,
      recursos: ['5 cotações / mês', '3 fornecedores', 'App Mobile', 'Suporte por e-mail'],
      ctaTexto: 'Começar grátis',
    },
    {
      id: 'profissional',
      nome: 'Profissional',
      descricao: 'Acelere seu fluxo de compras diário.',
      preco: 149,
      precoAnual: 119,
      badge: 'Mais popular',
      destaque: true,
      recursos: ['Cotações ilimitadas', 'Todos os fornecedores', 'Comparativo de marcas', 'Suporte via chat', 'Relatórios básicos'],
      ctaTexto: 'Assinar agora',
    },
    {
      id: 'business',
      nome: 'Business',
      descricao: 'Controle total para grandes frotas.',
      preco: 299,
      precoAnual: 239,
      badge: null,
      destaque: false,
      recursos: ['Múltiplos usuários', 'Gestão de NF-e', 'Suporte prioritário', 'Integração ERP básica', 'Relatórios avançados'],
      ctaTexto: 'Selecionar',
    },
    {
      id: 'enterprise',
      nome: 'Enterprise',
      descricao: 'Soluções customizadas para redes.',
      preco: null,
      precoAnual: null,
      badge: null,
      destaque: false,
      recursos: ['Integração via API', 'Key Account Manager', 'Customização de UI', 'SLA 99,9%', 'Usuários ilimitados'],
      ctaTexto: 'Falar com vendas',
    },
  ];

  readonly stats = [
    { valor: '50k+',  rotulo: 'Peças Cotadas/Mês' },
    { valor: '1.2k',  rotulo: 'Oficinas Ativas' },
    { valor: '450',   rotulo: 'Distribuidores' },
    { valor: 'R$ 12M+', rotulo: 'Volume Transacionado' },
  ];

  readonly metricas = [
    { valor: '30 min', rotulo: 'Tempo Médio de Cotação' },
    { valor: '3x',     rotulo: 'Mais Produtividade' },
    { valor: '100%',   rotulo: 'Online' },
    { valor: 'R$ 0',   rotulo: 'Taxa de Instalação' },
  ];

  readonly depoimentos = [
    {
      texto: '"Reduzi meu tempo de cotação em 70%. Hoje meus mecânicos focam apenas no carro, o administrativo ficou leve."',
      nome: 'Ricardo M.',
      empresa: 'Mecânica do Sol',
      iniciais: 'RM',
    },
    {
      texto: '"Como fornecedor, o Hub nos trouxe clientes que jamais alcançaríamos. O retorno sobre investimento é imediato."',
      nome: 'Amanda S.',
      empresa: 'Peças Prime Dist.',
      iniciais: 'AS',
    },
    {
      texto: '"A gestão de frotas se tornou muito mais transparente. Consigo auditar os preços e marcas instaladas com um clique."',
      nome: 'João P.',
      empresa: 'Logística Total',
      iniciais: 'JP',
    },
  ];

  readonly passosCompradores = [
    { tag: 'Solicitação',  titulo: 'Envie sua lista de peças', desc: 'Digite o código ou descreva a peça e o veículo. Nosso sistema identifica os fornecedores ideais na sua região.' },
    { tag: 'Processamento', titulo: 'Hub notifica o mercado', desc: 'Os fornecedores recebem sua cotação instantaneamente e respondem com preço e disponibilidade em tempo real.' },
    { tag: 'Comparação',   titulo: 'Escolha a melhor oferta', desc: 'Compare preços, marcas e prazos de entrega em uma única tela organizada por economia.' },
    { tag: 'Fechamento',   titulo: 'Pedido finalizado', desc: 'Confirme o pedido e receba as peças. Pagamento direto ou faturado conforme combinado.' },
  ];

  readonly passosFornecedores = [
    { tag: 'Cadastro',    titulo: 'Crie seu perfil de fornecedor', desc: 'Configure seu catálogo, regiões de atendimento e condições comerciais.' },
    { tag: 'Notificação', titulo: 'Receba cotações qualificadas', desc: 'Apenas compradores da sua região com perfil compatível enviam cotações para você.' },
    { tag: 'Resposta',    titulo: 'Responda em segundos', desc: 'Interface otimizada para resposta rápida com preço, prazo e marca diretamente no app.' },
    { tag: 'Venda',       titulo: 'Feche e entregue', desc: 'Gerencie pedidos, NF-e e entregas em um único painel. Seu volume de vendas cresce de forma escalável.' },
  ];

  readonly parceiros = ['BOSCH', 'MONROE', 'NAKATA', 'COFAP', 'MAHLE'];

  readonly problemasCards = [
    { icone: 'phone_missed', titulo: 'Telefones Ocupados', desc: 'Horas perdidas tentando contato com múltiplos vendedores para uma única cotação.' },
    { icone: 'schedule', titulo: 'Demora no Retorno', desc: 'A peça que você precisa agora só tem orçamento confirmado no final do dia.' },
    { icone: 'error_outline', titulo: 'Erros de Aplicação', desc: 'Informações desencontradas resultam em devoluções e carros parados no elevador.' },
    { icone: 'price_change', titulo: 'Falta de Comparação', desc: 'Você compra do primeiro que atende, sem saber se o preço é competitivo de verdade.' },
  ];

  @HostListener('window:scroll')
  onScroll(): void {
    this.navScrolled.set(window.scrollY > 32);
  }

  toggleCiclo(): void {
    this.cicloAnual.update((v) => !v);
  }

  setTab(tab: 'compradores' | 'fornecedores'): void {
    this.tabAtiva.set(tab);
  }

  toggleFaq(index: number): void {
    this.faqItems.update((items) =>
      items.map((item, i) => ({ ...item, aberto: i === index ? !item.aberto : false }))
    );
  }

  precoExibido(plano: Plano): string {
    if (plano.preco === null) return 'Grátis';
    const valor = this.cicloAnual() ? plano.precoAnual! : plano.preco;
    return `R$ ${valor}`;
  }

  scrollTo(id: string): void {
    document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' });
  }
}
